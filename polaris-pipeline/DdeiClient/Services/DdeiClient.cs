using Ddei.Factories.Contracts;
using Ddei.Exceptions;
using Domain.Exceptions;
using Ddei.Domain.CaseData.Args;
using Common.Dto.Case;
using DdeiClient.Services.Contracts;
using DdeiClient.Mappers.Contract;
using Common.Wrappers.Contracts;
using Ddei.Domain;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Mappers.Contracts;
using Microsoft.Extensions.Logging;
using Common.Exceptions;

namespace Ddei.Services
{
    public class DdeiClient : IDdeiClient
    {
        private readonly ICaseDataArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDdeiClientRequestFactory _ddeiClientRequestFactory;
        private readonly ILogger<DdeiClient> _logger;
        protected readonly HttpClient _httpClient;
        
        public DdeiClient(
            HttpClient httpClient,
            IDdeiClientRequestFactory ddeiClientRequestFactory,
            ICaseDataArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentMapper<DdeiCaseDocumentResponse> caseDocumentMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<DdeiClient> logger
            )
        {
            _caseDataServiceArgFactory = caseDataServiceArgFactory ?? throw new ArgumentNullException(nameof(caseDataServiceArgFactory));
            _caseDetailsMapper = caseDetailsMapper ?? throw new ArgumentNullException(nameof(caseDetailsMapper));
            _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _ddeiClientRequestFactory = ddeiClientRequestFactory ?? throw new ArgumentNullException(nameof(ddeiClientRequestFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg)
        {
            try
            {
                return await CallDdei<string>(_ddeiClientRequestFactory.CreateCmsModernTokenRequest(arg));
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCmsModernToken", exception);
            }
        }

        private async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiCmsUrnArgDto arg)
        {
            return await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiClientRequestFactory.CreateListCasesRequest(arg));
        }

        public async Task<IEnumerable<CaseDto>> ListCases(DdeiCmsUrnArgDto arg)
        {
            try
            {
                var caseIdentifiers = await ListCaseIdsAsync(arg);

                var calls = caseIdentifiers.Select(async caseIdentifier =>
                     await GetCaseAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

                var cases = await Task.WhenAll(calls);
                return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in ListCases", exception);
            }
        }

        public async Task<CaseDto> GetCase(DdeiCmsCaseArgDto arg)
        {
            try
            {
                var @case = await GetCaseAsync(arg);
                return _caseDetailsMapper.MapCaseDetails(@case);
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCase", exception);
            }
        }

        public async Task<DocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId)
        {
            var ddeiResults =  await CallDdei<List<DdeiCaseDocumentResponse>>(
                _ddeiClientRequestFactory.CreateListCaseDocumentsRequest(new DdeiCmsCaseArgDto{
                    Urn= caseUrn, 
                    CaseId = long.Parse(caseId), 
                    CmsAuthValues = cmsAuthValues, 
                    CorrelationId =  correlationId
                })
            );

            return ddeiResults
                .Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult))
                .ToArray();
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId)
        {
            var response = await CallDdei(
                _ddeiClientRequestFactory.CreateDocumentRequest(new DdeiCmsDocumentArgDto
                {
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = documentCategory,
                    DocumentId = int.Parse(documentId),
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = correlationId
                })
            );

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task CheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            try
            {
                await CallDdei(_ddeiClientRequestFactory.CreateCheckoutDocumentRequest(arg));
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            try
            {
                await CallDdei(_ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg));
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            try
            {
                await CallDdei(_ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream));
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in UploadPdf", exception);
            }
        }

        private async Task<DdeiCaseDetailsDto> GetCaseAsync(DdeiCmsCaseArgDto arg)
        {
            return await CallDdei<DdeiCaseDetailsDto>(_ddeiClientRequestFactory.CreateGetCaseRequest(arg));
        }

        private async Task<T> CallDdei<T>(HttpRequestMessage request)
        {
            using var response = await CallDdei(request);
            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> CallDdei(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(content);
                }
                return response;
            }
            catch (HttpRequestException exception)
            {
                throw new DdeiClientException(response.StatusCode, exception);
            }
        }
    }
}