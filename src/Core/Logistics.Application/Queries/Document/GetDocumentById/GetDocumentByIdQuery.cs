using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDocumentByIdQuery : IQuery<Result<DocumentDto>>
{
    public Guid DocumentId { get; set; }
}
