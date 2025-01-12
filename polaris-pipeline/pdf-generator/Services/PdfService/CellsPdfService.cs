﻿using System;
using System.IO;
using Aspose.Cells;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class CellsPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public CellsPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            using var workbook = _asposeItemFactory.CreateWorkbook(inputStream, correlationId);
            workbook.Save(pdfStream, new PdfSaveOptions { OnePagePerSheet = true });
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
