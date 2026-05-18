using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

public class GetContainerByIdQuery : IQuery<Result<ContainerDto>>
{
    public Guid Id { get; set; }
}
