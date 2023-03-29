﻿using System;
using Aspose.Pdf.Facades;
using Common.Dto.Request.Redaction;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface ICoordinateCalculator
    {
        RedactionCoordinatesDto CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinatesDto originatorCoordinates, PdfFileInfo targetPdfInfo, Guid correlationId);
    }
}
