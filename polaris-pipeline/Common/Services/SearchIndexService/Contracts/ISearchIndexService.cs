﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.CaseSearchService.Contracts
{
    public interface ISearchIndexService
    {
        Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

        Task<bool> WaitForStoreResultsAsync(AnalyzeResults analyzeResults, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId);

        Task<bool> WaitForCaseEmptyResultsAsync(long cmsCaseId, Guid correlationId);

        Task<IList<StreamlinedSearchLine>> QueryAsync(long caseId, List<SearchFilterDocument> documents, string searchTerm, Guid correlationId);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId);

        Task RemoveCaseIndexEntriesAsync(long caseId, Guid correlationId);

        Task RemoveDocumentIndexEntriesAsync(long caseId, string documentId, long versionId, Guid correlationId);

        Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId);
    }
}