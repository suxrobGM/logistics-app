using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>Raised at most once per stop.</summary>
public record TripStopRunningLateEvent(
    Guid TripId,
    Guid StopId,
    Guid LoadId,
    DateTime EstimatedArrivalAt,
    DateTime AppointmentEnd) : IDomainEvent;
