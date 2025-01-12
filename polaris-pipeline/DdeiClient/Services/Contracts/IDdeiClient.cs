using Common.Dto.Case;
using Common.Dto.Document;
using Ddei.Domain;
using Ddei.Domain.CaseData.Args;

namespace DdeiClient.Services.Contracts
{
    public interface IDdeiClient
    {
        Task<DdeiCmsAuthValuesDto> GetFullCmsAuthValues(DdeiCmsCaseDataArgDto arg);
        Task<DdeiCaseIdentifiersDto> GetUrnFromCaseId(DdeiCmsCaseIdArgDto arg);
        Task<IEnumerable<CaseDto>> ListCases(DdeiCmsUrnArgDto arg);
        Task<CaseDto> GetCase(DdeiCmsCaseArgDto arg);
        Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);
        Task<HttpResponseMessage> CheckoutDocument(DdeiCmsDocumentArgDto arg);
        Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg);
        Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream);
        Task<string> GetStatus();
    }
}