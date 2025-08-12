using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteDocumentCommand : IRequest<Result>
{
    public Guid DocumentId { get; set; }
}
