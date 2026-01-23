using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a load is removed from a truck (deletion or reassignment).
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record LoadRemovedFromTruckEvent(
    Guid LoadId,
    long LoadNumber,
    Guid TruckId,
    string TruckNumber,
    string? MainDriverDeviceToken,
    string? SecondaryDriverDeviceToken) : IDomainEvent;
