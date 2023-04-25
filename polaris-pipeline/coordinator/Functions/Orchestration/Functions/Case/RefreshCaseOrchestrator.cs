﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.Logging;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.Orchestration.Functions.Document;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Case
{
    public class RefreshCaseOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;

        const string loggingName = $"{nameof(RefreshCaseOrchestrator)} - {nameof(Run)}";

        public RefreshCaseOrchestrator(ILogger<RefreshCaseOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        [FunctionName(nameof(RefreshCaseOrchestrator))]
        public async Task<TrackerDeltasDto> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var currentCaseId = payload.CmsCaseId;

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var tracker = CreateOrGetTracker(context, payload.CmsCaseId, payload.CorrelationId, log);

            try
            {
                var timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
                var deadline = context.CurrentUtcDateTime.Add(timeout);

                using var cts = new CancellationTokenSource();
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Run main orchestration for case {currentCaseId}");
                var orchestratorTask = RunCaseOrchestrator(context, tracker, payload);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    return await orchestratorTask;
                }

                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                await tracker.RegisterFailed();
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<TrackerDeltasDto> RunCaseOrchestrator(IDurableOrchestrationContext context, ITrackerEntity tracker, CaseOrchestrationPayload payload)
        {
            const string loggingName = nameof(RunCaseOrchestrator);
            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Resetting tracker for {context.InstanceId}");
            await tracker.Reset(context.InstanceId);

            var (documents, pcdRequests) = await RetrieveDocumentsAndPcdRequests(context, tracker, loggingName, log, payload);
            var deltas = await SynchroniseTrackerDocuments(tracker, loggingName, log, payload, documents, pcdRequests);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"{deltas.CreatedDocuments.Count} CMS documents created, {deltas.UpdatedDocuments.Count} updated and {deltas.DeletedDocuments.Count} document deleted for case {payload.CmsCaseId}");
            var createdOrUpdatedDocuments = deltas.CreatedDocuments.Concat(deltas.UpdatedDocuments).ToList();
            var createdPcdRequests = deltas.CreatedPcdRequests;

            List<Task> caseTasks = GetCaseTasks(context, payload, createdOrUpdatedDocuments, createdPcdRequests);

            var changed = deltas.Any();

            if (changed)
            {
                await Task.WhenAll(caseTasks.Select(BufferCall));

                if (await tracker.AllDocumentsFailed())
                    throw new CaseOrchestrationException("Documents or PCD Requests failed to process during orchestration.");
            }

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Documents Refreshed, {deltas.CreatedDocuments.Count} created, {deltas.UpdatedDocuments.Count} updated, {deltas.DeletedDocuments.Count} deleted");
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"PCD Requests Refreshed, {deltas.CreatedPcdRequests.Count} created, {deltas.DeletedPcdRequests.Count} deleted");

            await tracker.RegisterCompleted();

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning changed documents");
            return deltas;
        }

        private static List<Task> GetCaseTasks
            (
                IDurableOrchestrationContext context, 
                CaseOrchestrationPayload caseDocumentPayload, 
                List<TrackerCmsDocumentDto> createdOrUpdatedDocuments,
                List<TrackerPcdRequestDto> createdPcdRequests
            )
        {
            var cmsDocumentPayloads
                = createdOrUpdatedDocuments
                    .Select
                    (
                        trackerCmsDocument =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                caseDocumentPayload.CmsAuthValues,
                                caseDocumentPayload.CorrelationId,
                                caseDocumentPayload.CmsCaseUrn,
                                caseDocumentPayload.CmsCaseId,
                                JsonSerializer.Serialize(trackerCmsDocument),
                                null
                            );
                        }
                    );

            var pcdRequestsPayloads
                = createdPcdRequests
                    .Select
                    (
                        trackerPcdRequest =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                caseDocumentPayload.CmsAuthValues,
                                caseDocumentPayload.CorrelationId,
                                caseDocumentPayload.CmsCaseUrn,
                                caseDocumentPayload.CmsCaseId,
                                null,
                                JsonSerializer.Serialize(trackerPcdRequest)
                            );
                        }
                    );

            var allPayloads = cmsDocumentPayloads.Concat( pcdRequestsPayloads );
            var allTasks = allPayloads.Select(payload => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), payload));

            return allTasks.ToList();
        }

        private static async Task BufferCall(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                return;
            }
        }

        private async Task<(DocumentDto[], PcdRequestDto[])> RetrieveDocumentsAndPcdRequests(IDurableOrchestrationContext context, ITrackerEntity tracker, string nameToLog, ILogger safeLogger, CaseOrchestrationPayload payload)
        {
            var getCaseEntitiesActivityPayload = new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId);

            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of Documents for case {payload.CmsCaseId}");
            var cmsDocuments = await context.CallActivityAsync<DocumentDto[]>(nameof(GetCaseDocuments), getCaseEntitiesActivityPayload);

            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of PCD Requests for case {payload.CmsCaseId}");
            var pcdRequests = await context.CallActivityAsync<PcdRequestDto[]>(nameof(GetCasePcdRequests), getCaseEntitiesActivityPayload);

            return (cmsDocuments, pcdRequests);
        }

        private static async Task<TrackerDeltasDto> SynchroniseTrackerDocuments
            (
                ITrackerEntity tracker, 
                string nameToLog, 
                ILogger safeLogger, 
                BasePipelinePayload payload, 
                DocumentDto[] documents,
                PcdRequestDto[] pcdRequests
            )
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Documents found, register document Ids in tracker for case {payload.CmsCaseId}");

            var arg = new SynchroniseDocumentsArg(payload.CmsCaseUrn, payload.CmsCaseId, documents, pcdRequests, payload.CorrelationId);
            var deltas = await tracker.SynchroniseDocuments(arg);

            return deltas;
        }
    }
}