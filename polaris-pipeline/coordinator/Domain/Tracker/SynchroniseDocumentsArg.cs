using Common.Dto.Document;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coordinator.Domain.Tracker;

public class SynchroniseDocumentsArg
{
    public SynchroniseDocumentsArg(string caseUrn, long caseId, TransitionDocumentDto[] documents, Guid correlationId)
    {
        CaseUrn = caseUrn ?? throw new ArgumentNullException(nameof(caseUrn));
        CaseId = caseId;
        Documents = documents?.ToList() ?? throw new ArgumentNullException(nameof(documents));
        CorrelationId = correlationId;
    }

    public string CaseUrn { get; set; }

    public long CaseId { get; set; }

    public List<TransitionDocumentDto> Documents { get; set; }

    public Guid CorrelationId { get; set; }
}
