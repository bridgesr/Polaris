﻿namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string BlobContainerName = "BlobContainerName";
    public const string BlobServiceUrl = "BlobServiceUrl";
    public const string BlobExpirySecs = "BlobExpirySecs";
    public const string BlobUserDelegationKeyExpirySecs = "BlobUserDelegationKeyExpirySecs";
    public const string TenantId = "OnBehalfOfTokenTenantId";
    public const string ClientId = "OnBehalfOfTokenClientId";
    public const string ClientSecret = "OnBehalfOfTokenClientSecret";
    public const string ValidAudience = "CallingAppValidAudience";
    public const string CoreDataApiScope = "CoreDataApiScope";
    public const string StubBlobStorageConnectionString = "StubBlobStorageConnectionString";
    public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
    public const string PipelineCoordinatorFunctionAppKey = "PolarisPipelineCoordinatorFunctionAppKey";
    public const string PipelineCoordinatorScope = "PolarisPipelineCoordinatorScope";
    public const string PipelineRedactPdfScope = "PolarisPipelineRedactPdfScope";
    public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
    public const string PipelineRedactPdfFunctionAppKey = "PolarisPipelineRedactPdfFunctionAppKey";
}

public static class AuthenticationKeys
{
    public const string Authorization = "Authorization";
    public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
    public const string AzureAuthenticationAssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
    public const string Bearer = "Bearer";
}

public static class ValidRoles
{
    public const string UserImpersonation = "user_impersonation";
}

public static class HttpHeaderKeys
{
    public const string CorrelationId = "Correlation-Id";
    public const string UpstreamToken = "Upstream-Token";
}
