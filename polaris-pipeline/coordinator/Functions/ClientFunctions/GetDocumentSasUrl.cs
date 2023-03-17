﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Services.SasGeneratorService;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;

namespace coordinator.Functions.ClientFunctions
{
    public class GetDocumentSasUrl : BaseClientFunction
    {
        private readonly ISasGeneratorService _sasGeneratorService;

        const string loggingName = $"{nameof(GetDocumentSasUrl)} - {nameof(HttpStart)}";

        public GetDocumentSasUrl(ISasGeneratorService sasGeneratorService)
        {
            _sasGeneratorService = sasGeneratorService;
        }

        [FunctionName(nameof(GetDocumentSasUrl))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.DocumentSasUrl)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId, 
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var response = await GetTrackerDocument(req, client, loggingName, caseId, documentId, log);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;
                var document = response.Document;
                var blobName = document.PdfBlobName;
                var sasUrl = await _sasGeneratorService.GenerateSasUrlAsync(blobName, currentCorrelationId);

                return !string.IsNullOrEmpty(sasUrl) ? new OkObjectResult(sasUrl) : null;
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
