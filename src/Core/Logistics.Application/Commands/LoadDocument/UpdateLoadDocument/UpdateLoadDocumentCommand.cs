using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateLoadDocumentCommand : IRequest<Result>
{
    public Guid DocumentId { get; set; }
    public DocumentType? Type { get; set; }
    public string? Description { get; set; }
    public Guid UpdatedById { get; set; }
}
