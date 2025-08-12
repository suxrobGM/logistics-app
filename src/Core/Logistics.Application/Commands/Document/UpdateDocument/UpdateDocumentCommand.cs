using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateDocumentCommand : IAppRequest
{
    public Guid DocumentId { get; set; }
    public DocumentType? Type { get; set; }
    public string? Description { get; set; }
    public Guid UpdatedById { get; set; }
}
