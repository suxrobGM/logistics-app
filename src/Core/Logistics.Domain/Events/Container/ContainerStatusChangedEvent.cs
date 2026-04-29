using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a container transitions between lifecycle states.
/// </summary>
public record ContainerStatusChangedEvent(
    Guid ContainerId,
    string ContainerNumber,
    ContainerStatus PreviousStatus,
    ContainerStatus NewStatus) : IDomainEvent;
