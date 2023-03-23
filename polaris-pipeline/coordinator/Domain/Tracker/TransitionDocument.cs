using Common.Domain.Case.Presentation;
using Common.Domain.DocumentExtraction;
using System;

namespace coordinator.Domain.Tracker;

public class TransitionDocument
{
    public TransitionDocument()
    {
        CmsDocType = new CmsDocType();
    }
    public TransitionDocument(
        Guid polarisDocumentId,
        string documentId,
        long versionId,
        string originalFileName,
        string mimeType,
        string fileExtension,
        CmsDocType cmsDocType,
        string createdDate)
    {
        PolarisDocumentId = polarisDocumentId;
        DocumentId = documentId;
        VersionId = versionId;
        OriginalFileName = originalFileName;
        MimeType = mimeType;
        FileExtension = fileExtension;
        CmsDocType = cmsDocType;
        CreatedDate = createdDate;
    }

    public Guid PolarisDocumentId { get; init; }

    public string DocumentId { get; init; }

    public long VersionId { get; set; }

    public string OriginalFileName { get; init; }

    public string MimeType { get; init; }

    public string FileExtension { get; set; }

    public CmsDocType CmsDocType { get; set; }

    public string CreatedDate { get; set; }

    public PresentationFlags PresentationFlags { get; set; }
}
