﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Logging;
using Common.Mappers.Contracts;
using Common.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Common.Mappers
{
    public class RedactPdfRequestMapper : IRedactPdfRequestMapper
    {
        private readonly ILogger<RedactPdfRequestMapper> _logger;

        public RedactPdfRequestMapper(ILogger<RedactPdfRequestMapper> logger)
        {
            _logger = logger;
        }

        public RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Map), $"SaveRequest: '{saveRequest.ToJson()}', CaseId: {caseId}, PolarisDocumentId: {polarisDocumentId}");

            if (saveRequest == null) throw new ArgumentNullException(nameof(saveRequest));

            var result = new RedactPdfRequestDto
            {
                CaseId = caseId,
                PolarisDocumentId = polarisDocumentId,
                // FileName - not known yet, picked up later in the durable world
                // VersionId - not passed in previous code, possibly get set as 0->1 in Bob metadata, but as not used this isn't a problem
                RedactionDefinitions = new List<RedactionDefinitionDto>()
            };

            _logger.LogMethodFlow(correlationId, nameof(Map), "Mapping each set of redaction details (co-ordinates and page info) to an object that the PDFGenerator pipeline API expects");
            foreach (var item in saveRequest.Redactions)
            {
                var redactionDefinition = new RedactionDefinitionDto
                {
                    PageIndex = item.PageIndex,
                    Height = item.Height,
                    Width = item.Width,
                    RedactionCoordinates = new List<RedactionCoordinatesDto>()
                };
                foreach (var redactionCoordinates in item.RedactionCoordinates.Select(coordinates => new RedactionCoordinatesDto
                {
                    X1 = coordinates.X1,
                    Y1 = coordinates.Y1,
                    X2 = coordinates.X2,
                    Y2 = coordinates.Y2
                }))
                {
                    redactionDefinition.RedactionCoordinates.Add(redactionCoordinates);
                }

                result.RedactionDefinitions.Add(redactionDefinition);
            }

            _logger.LogMethodExit(correlationId, nameof(Map), result.ToJson());
            return result;
        }
    }
}
