using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetLoadByIdQuery : IQuery<Result<LoadDto>>
{
    public Guid Id { get; set; }
}
