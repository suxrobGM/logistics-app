using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDocumentByIdQuery : IAppRequest<Result<DocumentDto>>
{
    public Guid DocumentId { get; set; }
}
