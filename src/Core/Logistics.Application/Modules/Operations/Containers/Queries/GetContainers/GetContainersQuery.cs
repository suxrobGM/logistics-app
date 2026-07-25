using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class GetContainersQuery : SearchableQuery, IQuery<PagedResult<ContainerDto>>
{
    public ContainerStatus? Status { get; set; }
    public ContainerIsoType? IsoType { get; set; }
    public Guid? CurrentTerminalId { get; set; }
}
