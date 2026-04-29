namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Lifecycle states for an intermodal container.
/// </summary>
public enum ContainerStatus
{
    Empty,
    Loaded,
    AtPort,
    InTransit,
    Delivered,
    Returned
}
