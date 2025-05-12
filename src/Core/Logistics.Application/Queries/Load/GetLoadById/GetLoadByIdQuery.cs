using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetLoadByIdQuery : IRequest<Result<LoadDto>>
{
    public Guid Id { get; set; }
}
