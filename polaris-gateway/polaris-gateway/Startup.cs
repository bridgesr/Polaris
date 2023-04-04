﻿using Common.Constants;
using Common.Domain.Extensions;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Health;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Validators;
using Common.Validators.Contracts;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Ddei.Services;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using Ddei.Mappers;
using Ddei.Options;
using DdeiClient.Services.Contracts;
using DdeiClient.Mappers.Contract;
using Gateway.Clients.PolarisPipeline;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.common.Mappers.Contracts;
using PolarisGateway.Factories;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Mappers;
using PolarisGateway.Wrappers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Headers;
using Ddei.Services.Extensions;

[assembly: FunctionsStartup(typeof(PolarisGateway.Startup))]

namespace PolarisGateway
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
            IdentityModelEventSource.ShowPII = true;
#endif

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<ITriggerCoordinatorResponseFactory, TriggerCoordinatorResponseFactory>();
            builder.Services.AddTransient<ITrackerUrlMapper, TrackerUrlMapper>();

            builder.Services.AddHttpClient<IPipelineClient, PipelineClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, PipelineSettings.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();

            builder.Services.AddDdeiClient(configuration);
            builder.Services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();

            BuildHealthChecks(builder);
        }

        // see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        // Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            var pipelineCoordinator = "pipelineCoordinator";
            builder.Services.AddHttpClient(pipelineCoordinator, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineCoordinatorBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var pdfFunctions = "pdfFunctions";
            builder.Services.AddHttpClient(pdfFunctions, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineRedactPdfBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var ddeiClient = "ddeiClient";

            builder.Services.AddHealthChecks()
                .AddCheck<DdeiClientHealthCheck>(ddeiClient)
                .AddTypeActivatedCheck<AzureFunctionHealthCheck>("Pipeline co-ordinator", args: new object[] { pipelineCoordinator });
        }

        private static string GetValueFromConfig(IConfiguration configuration, string secretName)
        {
            var secret = configuration[secretName];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new Exception($"Secret cannot be null: {secretName}");
            }

            return secret;
        }
    }
}
