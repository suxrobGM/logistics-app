using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// The ETA for a stop passed the end of its appointment window by more than the grace period.
/// Raised at most once per stop.
/// </summary>
public record TripStopRunningLateEvent(
    Guid TripId,
    Guid StopId,
    Guid LoadId,
    DateTime EstimatedArrivalAt,
    DateTime AppointmentEnd) : IDomainEvent;
