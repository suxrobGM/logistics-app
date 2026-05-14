using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetContainerByIdQuery : IQuery<Result<ContainerDto>>
{
    public Guid Id { get; set; }
}
