using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Pure location update - moves the container to a terminal without changing its lifecycle status.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class MoveContainerToTerminalCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public Guid TerminalId { get; set; }
}
