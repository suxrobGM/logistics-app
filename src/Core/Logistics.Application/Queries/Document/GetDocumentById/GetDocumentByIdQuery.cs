using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetDocumentByIdQuery : IRequest<Result<DocumentDto>>
{
    public Guid DocumentId { get; set; }
}
