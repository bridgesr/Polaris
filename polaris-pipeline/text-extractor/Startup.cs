﻿using Common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Services.OcrService;
using Common.Services.SasGeneratorService;
using Azure.Identity;
using System;
using System.Diagnostics.CodeAnalysis;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Services.CaseSearchService;
using Common.Factories;
using Common.Health;
using Common.Services.Extensions;
using Common.Wrappers.Contracts;
using System.IO;
using Common.Dto.Request;
using Common.Handlers.Contracts;
using Common.Handlers;
using Common.Telemetry.Contracts;
using Common.Telemetry;

[assembly: FunctionsStartup(typeof(text_extractor.Startup))]
namespace text_extractor
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            BuildOcrService(builder, configuration);
            builder.Services.AddSearchClient(configuration);
            builder.Services.AddTransient<ISasGeneratorService, SasGeneratorService>();
            BuildAzureClients(builder, configuration);
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IValidatorWrapper<ExtractTextRequestDto>, ValidatorWrapper<ExtractTextRequestDto>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();

            builder.Services.AddBlobSasGenerator();
            builder.Services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            builder.Services.AddSingleton<ITelemetryClient, TelemetryClient>();

            BuildHealthChecks(builder);
        }

        private static void BuildAzureClients(IFunctionsHostBuilder builder, IConfigurationRoot configuration)
        {
            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
                string blobServiceUrl = configuration[ConfigKeys.SharedKeys.BlobServiceUrl];
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl))
                    .WithCredential(new DefaultAzureCredential());
            });
        }

        private static void BuildOcrService(IFunctionsHostBuilder builder, IConfigurationRoot configuration)
        {
#if DEBUG
            if (configuration.IsSettingEnabled(DebugSettings.MockOcrService))
            {
                builder.Services.AddSingleton<IOcrService, MockOcrService>();
            }
            else
            {
                builder.Services.AddSingleton<IOcrService, OcrService>();
            }
#else
            builder.Services.AddSingleton<IOcrService, OcrService>();
#endif
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client")
                .AddCheck<OcrServiceHealthCheck>("OCR Service")
                .AddCheck<AzureComputerVisionClientHealthCheck>("OCR Service / Azure Computer Vision Client")
                .AddCheck<SearchIndexServiceHealthCheck>("Search Index Service")
                .AddCheck<AzureSearchClientHealthCheck>("Search Index Service / Azure Search Client");
        }
    }
}