using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

public class GetContainerByIdQuery : IQuery<Result<ContainerDto>>, IHaveId
{
    public Guid Id { get; set; }
}
