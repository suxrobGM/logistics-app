using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetLoadByIdQuery : IRequest<Result<LoadDto>>
{
    public string Id { get; set; } = null!;
}
