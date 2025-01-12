using System;

namespace coordinator.Domain
{
    public class CaseOrchestrationPayload : BasePipelinePayload
    {
        public CaseOrchestrationPayload(string cmsCaseUrn, long cmsCaseId, string baseUrl, string extensionCode, string cmsAuthValues, Guid correlationId)
            : base(cmsCaseUrn, cmsCaseId, correlationId)
        {
            BaseUrl = baseUrl;
            CmsAuthValues = cmsAuthValues;
            ExtensionCode = extensionCode;
        }

        public string BaseUrl { get; init; }
        public string ExtensionCode { get; init; }
        public string AccessToken { get; init; }
        public string CmsAuthValues { get; init; }
    }
}