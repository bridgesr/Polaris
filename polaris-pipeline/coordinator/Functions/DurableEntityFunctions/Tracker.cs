﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Pipeline;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Functions.DurableEntityFunctions
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Tracker : ITracker
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocument> Documents { get; set; }

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }

        public Task Reset(string transactionId)
        {
            TransactionId = transactionId;

            Status = TrackerStatus.Running;
            if(Documents == null)
                Documents = new List<TrackerDocument>();
            Logs = new List<Log>();
            ProcessingCompleted = null; //reset the processing date

            Log(LogType.Initialised);

            return Task.CompletedTask;
        }

        public Task SetValue(Tracker tracker)
        {
            this.Status = tracker.Status;
            this.ProcessingCompleted = tracker.ProcessingCompleted;
            this.Documents = tracker.Documents;
            this.Logs = new List<Log>();

            return Task.CompletedTask;
        }

        public Task SynchroniseDocuments(SynchroniseDocumentsArg arg)
        {
            if (Documents.Count == 0) //no documents yet loaded in the tracker for this case, grab them all
            {
                Documents = arg.Documents
                                .Select(item => CreateTrackerDocument(item))
                                .ToList();
                Log(LogType.DocumentsSynchronised);
            }
            else
            {
                List<DocumentToRemove> documentsToRemove = GetDocumentsToRemove(arg);

                RemoveDocuments(documentsToRemove);

                var newDocuments = GetNewDocuments(arg.Documents, documentsToRemove);

                foreach (var cmsDocument in newDocuments)
                {
                    TrackerDocument trackerDocument = CreateTrackerDocument(cmsDocument);
                    Documents.Add(trackerDocument);
                    Log(LogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
                }
            }

            Status = TrackerStatus.DocumentsRetrieved;

            return Task.CompletedTask;
        }

        private List<DocumentToRemove> GetDocumentsToRemove(SynchroniseDocumentsArg arg)
        {
            List<DocumentToRemove> documentsToRemove = new List<DocumentToRemove>();
            foreach (var trackedDocument in
                     Documents.Where(trackedDocument =>
                         !arg.Documents.Exists(x => x.DocumentId == trackedDocument.CmsDocumentId && x.VersionId == trackedDocument.CmsVersionId)))
            {
                documentsToRemove.Add(new DocumentToRemove(trackedDocument.CmsDocumentId, trackedDocument.CmsVersionId));
            }

            return documentsToRemove;
        }

        private IEnumerable<TransitionDocument> GetNewDocuments(List<TransitionDocument> transitionDocuments, List<DocumentToRemove> documentsToRemove)
        {
            var newDocuments = from cmsDocument in transitionDocuments
                               where !documentsToRemove.Exists
                               (
                                   x => x.DocumentId == cmsDocument.DocumentId &&
                                        x.VersionId == cmsDocument.VersionId
                               )
                               let item = Documents.Find(x => x.CmsDocumentId == cmsDocument.DocumentId)
                               where item == null
                               select cmsDocument;
            return newDocuments;
        }

        private void RemoveDocuments(List<DocumentToRemove> documentsToRemove)
        {
            foreach (var item in
                     documentsToRemove.Select(invalidDocument =>
                         Documents.Find(x => x.CmsDocumentId == invalidDocument.DocumentId && x.CmsVersionId == invalidDocument.VersionId)))
            {
                Documents.Remove(item);
            }
        }

        private TrackerDocument CreateTrackerDocument(TransitionDocument document)
        {
            return new TrackerDocument(
                document.PolarisDocumentId,
                document.DocumentId,
                document.VersionId,
                document.CmsDocType,
                document.MimeType,
                document.FileExtension,
                document.CreatedDate,
                document.OriginalFileName,
                document.PresentationFlags);
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = DocumentStatus.PdfUploadedToBlob;
                document.IsPdfAvailable = true;
            }

            Log(LogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = DocumentStatus.DocumentAlreadyProcessed;
            }

            Log(LogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.UnableToConvertToPdf;

            Log(LogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.Indexed;

            Log(LogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = DocumentStatus.OcrAndIndexFailure;

            Log(LogType.OcrAndIndexFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Completed;
            Log(LogType.Completed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterFailed()
        {
            Status = TrackerStatus.Failed;
            Log(LogType.Failed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterDeleted()
        {
            ClearState(TrackerStatus.Deleted);
            Log(LogType.Deleted);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task ClearDocuments()
        {
            Documents.Clear();

            return Task.CompletedTask;
        }

        public Task<bool> AllDocumentsFailed()
        {
            return Task.FromResult(
                Documents.All(d => d.Status is DocumentStatus.UnableToConvertToPdf or DocumentStatus.UnexpectedFailure));
        }

        /*public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status is TrackerStatus.Completed);
        }

        public Task<bool> IsStale()
        {
            if (Status is TrackerStatus.Running)
                return Task.FromResult(false);

            if (Status is TrackerStatus.Failed)
                return Task.FromResult(true);

            return ProcessingCompleted.HasValue
                ? Task.FromResult(ProcessingCompleted.Value.Date != DateTime.Now.Date)
                : Task.FromResult(false);
        }*/

        private void ClearState(TrackerStatus status)
        {
            Status = status;
            Documents = new List<TrackerDocument>();
            Logs = new List<Log>();
            ProcessingCompleted = null; //reset the processing date
        }

        private void Log(LogType status, string cmsDocumentId = null)
        {
            Logs.Add(new Log
            {
                LogType = status.ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                CmsDocumentId = cmsDocumentId
            });
        }

        [FunctionName("Tracker")]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Tracker>();
        }
    }
}