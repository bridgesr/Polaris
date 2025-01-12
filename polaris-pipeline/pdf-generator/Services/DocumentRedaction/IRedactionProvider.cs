using System;
using System.IO;
using Common.Dto.Request;

namespace pdf_generator.Services.DocumentRedaction
{
    public interface IRedactionProvider
    {
        Stream Redact(Stream stream, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}