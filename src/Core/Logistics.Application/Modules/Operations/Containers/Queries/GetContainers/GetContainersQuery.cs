using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

public class GetContainersQuery : SearchableQuery, IQuery<PagedResult<ContainerDto>>
{
    public ContainerStatus? Status { get; set; }
    public ContainerIsoType? IsoType { get; set; }
    public Guid? CurrentTerminalId { get; set; }
}
