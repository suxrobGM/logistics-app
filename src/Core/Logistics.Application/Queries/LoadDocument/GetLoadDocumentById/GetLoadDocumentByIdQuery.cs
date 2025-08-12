using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetLoadDocumentByIdQuery : IRequest<Result<LoadDocumentDto>>
{
    public Guid DocumentId { get; set; }
}
