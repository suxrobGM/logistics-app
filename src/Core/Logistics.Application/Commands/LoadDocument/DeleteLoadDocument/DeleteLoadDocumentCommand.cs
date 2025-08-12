using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteLoadDocumentCommand : IRequest<Result>
{
    public Guid DocumentId { get; set; }
    public Guid RequestedById { get; set; }
}
