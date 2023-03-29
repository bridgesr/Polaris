﻿using System;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}
