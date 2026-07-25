using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class LinkContainerToLoadCommand : ICommand<Result>
{
    public Guid ContainerId { get; set; }
    public Guid LoadId { get; set; }
}
