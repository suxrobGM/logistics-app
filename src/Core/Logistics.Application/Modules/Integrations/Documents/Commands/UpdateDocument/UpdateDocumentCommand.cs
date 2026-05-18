using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

public class UpdateDocumentCommand : ICommand
{
    public Guid DocumentId { get; set; }
    public DocumentType? Type { get; set; }
    public string? Description { get; set; }
    public Guid UpdatedById { get; set; }
}
