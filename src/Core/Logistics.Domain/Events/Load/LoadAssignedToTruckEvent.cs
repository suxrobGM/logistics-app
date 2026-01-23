using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a load is assigned to a truck (either at creation or via update).
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record LoadAssignedToTruckEvent(
    Guid LoadId,
    long LoadNumber,
    Guid TruckId,
    string TruckNumber,
    string? MainDriverDeviceToken,
    string? SecondaryDriverDeviceToken,
    string DriverDisplayName) : IDomainEvent;
