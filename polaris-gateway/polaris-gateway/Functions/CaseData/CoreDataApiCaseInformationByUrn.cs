﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PolarisGateway.Clients.OnBehalfOfTokenClient;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using PolarisGateway.Services;
using PolarisGateway.Factories;
using System.Net;
using Microsoft.Extensions.Options;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using PolarisGateway.Domain.Exceptions;

namespace PolarisGateway.Functions.CaseDataApi.Case
{
    public class CaseDataApiCaseInformationByUrn : BasePolarisFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseInformationByUrn> _logger;
        private readonly DdeiOptions _tdeOptions;

        public CaseDataApiCaseInformationByUrn(ILogger<CaseDataApiCaseInformationByUrn> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient, ICaseDataService caseDataService,
                                 IAuthorizationValidator tokenValidator, ICaseDataArgFactory caseDataArgFactory, IOptions<DdeiOptions> options)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
            _tdeOptions = options.Value;
        }

        [FunctionName("CaseDataApiCaseInformationByUrn")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases")] HttpRequest req,
            string urn)
        {
            Guid currentCorrelationId = default;
            string upstreamToken = null;
            const string loggingName = "CaseDataApiCaseInformationByUrn - Run";
            IEnumerable<CaseDetails> caseInformation = null;

            try
            {
                urn = WebUtility.UrlDecode(urn); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                upstreamToken = validationResult.UpstreamToken;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrEmpty(urn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                //var cdaScope = _configuration[ConfigurationKeys.CoreDataApiScope];
                //_logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {cdaScope}");
                var onBehalfOfAccessToken = "not-implemented-yet"; // await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), cdaScope, currentCorrelationId);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case information by Urn '{urn}'");
                var urnArg = _caseDataArgFactory.CreateUrnArg(onBehalfOfAccessToken, upstreamToken, currentCorrelationId, urn);
                caseInformation = await _caseDataService.ListCases(urnArg);

                if (caseInformation != null && caseInformation.Any())
                {
                    return new OkObjectResult(caseInformation);
                }

                return NotFoundErrorResponse($"No data found for urn '{urn}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An MSAL exception occurred.", currentCorrelationId, loggingName),
                    CaseDataServiceException => InternalServerErrorResponse(exception, "A case data api exception occurred.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, caseInformation.ToJson());
            }
        }
    }
}
