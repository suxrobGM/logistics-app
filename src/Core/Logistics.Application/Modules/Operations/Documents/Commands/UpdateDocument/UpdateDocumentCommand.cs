using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Documents.Commands;

public class UpdateDocumentCommand : ICommand
{
    public Guid DocumentId { get; set; }
    public DocumentType? Type { get; set; }
    public string? Description { get; set; }
}
