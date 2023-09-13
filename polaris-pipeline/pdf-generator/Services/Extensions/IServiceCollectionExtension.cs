﻿using Aspose.Imaging.MemoryManagement;
using Castle.Core.Configuration;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.PdfService;
using System.Linq;

namespace pdf_generator.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddPdfGenerator(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IPdfService, WordsPdfService>();
            services.AddSingleton<IPdfService, CellsPdfService>();
            services.AddSingleton<IPdfService, SlidesPdfService>();
            services.AddSingleton<IPdfService, ImagingPdfService>();
            services.AddSingleton<IPdfService, DiagramPdfService>();
            services.AddSingleton<IPdfService, HtmlPdfService>();
            services.AddSingleton<IPdfService, EmailPdfService>();
            services.AddSingleton<IPdfService, PdfRendererService>();
            services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var pdfServices = provider.GetServices<IPdfService>();
                var servicesList = pdfServices.ToList();
                var wordsPdfService = servicesList.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = servicesList.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = servicesList.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = servicesList.First(s => s.GetType() == typeof(DiagramPdfService));
                var htmlPdfService = servicesList.First(s => s.GetType() == typeof(HtmlPdfService));
                var emailPdfService = servicesList.First(s => s.GetType() == typeof(EmailPdfService));
                var pdfRendererService = servicesList.First(s => s.GetType() == typeof(PdfRendererService));
                var loggingService = provider.GetService<ILogger<PdfOrchestratorService>>();

                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService,
                    diagramPdfService, htmlPdfService, emailPdfService, pdfRendererService, loggingService, configuration);
            });

            services.AddTransient<ICoordinateCalculator, CoordinateCalculator>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
        }
    }
}