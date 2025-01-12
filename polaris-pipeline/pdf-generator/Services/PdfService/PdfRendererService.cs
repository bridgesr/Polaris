﻿using System;
using System.IO;
using Aspose.Pdf;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService;

public class PdfRendererService : IPdfService
{
    private readonly IAsposeItemFactory _asposeItemFactory;

    public PdfRendererService(IAsposeItemFactory asposeItemFactory)
    {
        _asposeItemFactory = asposeItemFactory;
    }

    public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
    {
        var doc = _asposeItemFactory.CreateRenderedPdfDocument(inputStream, correlationId);
        doc.Save(pdfStream, SaveFormat.Pdf);
        pdfStream.Seek(0, SeekOrigin.Begin);
    }
}
