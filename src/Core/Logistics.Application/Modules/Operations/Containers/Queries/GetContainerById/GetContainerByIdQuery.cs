using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class GetContainerByIdQuery : IQuery<Result<ContainerDto>>, IHaveId
{
    public Guid Id { get; set; }
}
