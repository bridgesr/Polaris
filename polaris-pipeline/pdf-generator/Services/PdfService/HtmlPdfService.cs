﻿using System;
using System.IO;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class HtmlPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public HtmlPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            try
            {
                using var doc = _asposeItemFactory.CreateHtmlDocument(inputStream, correlationId);
                doc.Save(pdfStream);
                pdfStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception)
            {
#if DEBUG
                // we are here because at the time of writing generating a pdf from html blows up when developing on a mac
                using var fs = new FileStream("development-time.pdf", FileMode.Open, FileAccess.Read);
                fs.CopyTo(pdfStream);
                pdfStream.Seek(0, SeekOrigin.Begin);
#else
                throw;
#endif
            }
        }
    }
}
