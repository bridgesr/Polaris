﻿using System;
using System.IO;
using Aspose.Imaging.FileFormats.Pdf;
using Aspose.Imaging.ImageOptions;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class ImagingPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public ImagingPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            using var image = _asposeItemFactory.CreateImage(inputStream, correlationId);
            image.Save(pdfStream, new PdfOptions { PdfDocumentInfo = new PdfDocumentInfo() });
            pdfStream.Seek(0, System.IO.SeekOrigin.Begin);
        }
    }
}
