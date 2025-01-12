using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;

namespace Common.Clients
{
    public class TextExtractorClient : ITextExtractorClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly IPipelineClientSearchRequestFactory _pipelineClientSearchRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TextExtractorClient(
            HttpClient httpClient,
            IConfiguration configuration,
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            IPipelineClientSearchRequestFactory pipelineClientSearchRequestFactory,
            IJsonConvertWrapper jsonConvertWrapper
            )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _pipelineClientSearchRequestFactory = pipelineClientSearchRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task ExtractTextAsync(
            PolarisDocumentId polarisDocumentId,
            long cmsCaseId,
            string cmsDocumentId,
            long versionId,
            string blobName,
            Guid correlationId,
            Stream documentStream)
        {
            // For integration testing allow disabling of text extraction
            if (_configuration.IsConfigSettingEnabled(FeatureFlags.DisableTextExtractorFeatureFlag))
            {
                // Use average time seen in production for text extraction to simulate time typically taken
                Thread.Sleep(8000);
                return;
            }

            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.Extract}?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.PolarisDocumentId, polarisDocumentId.ToString());
            request.Headers.Add(HttpHeaderKeys.CaseId, cmsCaseId.ToString());
            request.Headers.Add(HttpHeaderKeys.DocumentId, cmsDocumentId);
            request.Headers.Add(HttpHeaderKeys.VersionId, versionId.ToString());
            request.Headers.Add(HttpHeaderKeys.BlobName, blobName);

            using (var requestContent = new StreamContent(documentStream))
            {
                request.Content = requestContent;

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task<IList<StreamlinedSearchLine>> SearchTextAsync(
            long cmsCaseId,
            string searchTerm,
            Guid correlationId,
            IEnumerable<SearchFilterDocument> documents
            )
        {
            var request = _pipelineClientSearchRequestFactory.Create(cmsCaseId, searchTerm, correlationId, documents);
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(content);
            }
        }
    }
}