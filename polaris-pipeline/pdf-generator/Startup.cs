﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers;
using Common.Handlers.Contracts;
using Common.Health;
using Common.Services.DocumentEvaluation;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.Extensions;
using Common.Telemetry;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers;
using Common.Telemetry.Wrappers.Contracts;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.Extensions;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
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
            builder.Services.AddBlobStorageWithDefaultAzureCredential(configuration);

            builder.Services.AddPdfGenerator();

            builder.Services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
            builder.Services.AddTransient<IDocumentRedactionService, DocumentRedactionService>();
            builder.Services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddSingleton<ITelemetryClient, TelemetryClient>();
            builder.Services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            BuildHealthChecks(builder);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHealthChecks()
                 .AddCheck<AzureSearchClientHealthCheck>("Azure Search Client")
                 .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client");
        }
    }
}