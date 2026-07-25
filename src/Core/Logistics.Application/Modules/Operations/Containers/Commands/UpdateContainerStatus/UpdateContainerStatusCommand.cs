using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Transitions a container to a target lifecycle status.
/// <see cref="TerminalId"/> is required for transitions to <c>AtPort</c> and <c>Returned</c>;
/// ignored for other targets.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class UpdateContainerStatusCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public ContainerStatus TargetStatus { get; set; }
    public Guid? TerminalId { get; set; }
}
