using Common.Dto.Document;
using coordinator.Domain.Tracker;

namespace coordinator.Mappers
{
    public interface ITransitionDocumentMapper
    {
        TransitionDocument MapToTransitionDocument(DocumentDto cmsCaseDocument);
    }
}