using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a trip is assigned to a truck (first-time assignment or reassignment).
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record TripAssignedToTruckEvent(
    Guid TripId,
    long TripNumber,
    string TripName,
    Guid NewTruckId,
    string NewTruckNumber,
    string? NewDriverDeviceToken,
    string NewDriverDisplayName,
    Guid? OldTruckId,
    string? OldDriverDeviceToken) : IDomainEvent;
