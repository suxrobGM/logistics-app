using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetLoadDocumentsQuery : IRequest<Result<IEnumerable<LoadDocumentDto>>>
{
    public Guid LoadId { get; set; }
    public DocumentStatus? Status { get; set; }
    public DocumentType? Type { get; set; }
}
