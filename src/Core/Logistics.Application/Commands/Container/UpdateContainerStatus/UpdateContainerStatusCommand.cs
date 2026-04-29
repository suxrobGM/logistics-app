using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Transitions a container to a target lifecycle status.
/// <see cref="TerminalId"/> is required for transitions to <c>AtPort</c> and <c>Returned</c>;
/// ignored for other targets.
/// </summary>
public class UpdateContainerStatusCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public ContainerStatus TargetStatus { get; set; }
    public Guid? TerminalId { get; set; }
}
