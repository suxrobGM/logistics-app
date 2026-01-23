using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when load details are updated (same truck assignment).
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record LoadUpdatedEvent(
    Guid LoadId,
    long LoadNumber,
    Guid TruckId,
    string TruckNumber,
    string? MainDriverDeviceToken,
    string? SecondaryDriverDeviceToken) : IDomainEvent;
