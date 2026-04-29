using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetContainersQuery : SearchableQuery, IAppRequest<PagedResult<ContainerDto>>
{
    public ContainerStatus? Status { get; set; }
    public ContainerIsoType? IsoType { get; set; }
    public Guid? CurrentTerminalId { get; set; }
}
