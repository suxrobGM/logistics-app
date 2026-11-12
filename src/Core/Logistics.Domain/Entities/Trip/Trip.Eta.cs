using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Arrival estimates for a trip's remaining stops.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// How often a trip's ETAs are recomputed from a stream of truck positions.
    /// </summary>
    public static readonly TimeSpan EtaRefreshInterval = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How far past the appointment end an ETA may run before the stop counts as late.
    /// </summary>
    public static readonly TimeSpan LateGracePeriod = TimeSpan.FromMinutes(30);

    /// <summary>
    /// When the stop ETAs were last recomputed.
    /// </summary>
    public DateTime? EtaUpdatedAt { get; private set; }

    /// <summary>
    /// When the driver was found short of the hours needed to finish this trip; at most one flag per trip.
    /// </summary>
    public DateTime? HosShortfallFlaggedAt { get; set; }

    public bool IsActive => Status is TripStatus.Dispatched or TripStatus.InTransit;

    public bool IsEtaDue(DateTime now)
    {
        return EtaUpdatedAt is not { } updatedAt || now - updatedAt >= EtaRefreshInterval;
    }

    /// <summary>
    /// The stops the truck has not arrived at yet, in route order.
    /// </summary>
    public IReadOnlyList<TripStop> GetRemainingStops()
    {
        return [.. Stops.Where(s => s.ArrivedAt is null).OrderBy(s => s.Order)];
    }

    /// <summary>
    /// Stores the new arrival estimates and raises <see cref="TripStopRunningLateEvent"/> for each stop
    /// whose estimate first runs past its appointment end plus <see cref="LateGracePeriod"/>.
    /// </summary>
    public void ApplyArrivalEstimates(IReadOnlyDictionary<Guid, DateTime> etaByStopId, DateTime now)
    {
        EtaUpdatedAt = now;

        foreach (var stop in Stops)
        {
            if (!etaByStopId.TryGetValue(stop.Id, out var eta))
            {
                continue;
            }

            stop.EstimatedArrivalAt = eta;
            MarkLateIfPastAppointment(stop, eta, now);
        }
    }

    private void MarkLateIfPastAppointment(TripStop stop, DateTime eta, DateTime now)
    {
        if (stop.LateNotifiedAt is not null || stop.GetAppointmentEnd() is not { } appointmentEnd)
        {
            return;
        }

        if (eta <= appointmentEnd + LateGracePeriod)
        {
            return;
        }

        stop.LateNotifiedAt = now;
        DomainEvents.Add(new TripStopRunningLateEvent(Id, stop.Id, stop.LoadId, eta, appointmentEnd));
    }
}
