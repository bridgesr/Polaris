﻿using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Domain.SearchIndex;
using Microsoft.AspNetCore.Mvc;
using PolarisGateway.Domain.PolarisPipeline;

namespace Gateway.Clients.PolarisPipeline.Contracts
{
    public interface IPipelineClient
    {
        Task TriggerCoordinatorAsync(string caseUrn, int caseId, string cmsAuthValues, bool force, Guid correlationId);
        Task<Tracker> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId);
        Task<string> GenerateDocumentSasUrlAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId);
        Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task<IActionResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, Guid polarisDocumentId, RedactPdfRequest redactPdfRequest, string cmsAuthValues, Guid correlationId);
        Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    }
}