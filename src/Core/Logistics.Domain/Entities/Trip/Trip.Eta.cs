using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class Trip
{
    /// <summary>Truck positions arrive far more often than an ETA needs recomputing.</summary>
    public static readonly TimeSpan EtaRefreshInterval = TimeSpan.FromMinutes(10);

    /// <summary>How far an ETA may run past the appointment end before the stop counts as late.</summary>
    public static readonly TimeSpan LateGracePeriod = TimeSpan.FromMinutes(30);

    public DateTime? EtaUpdatedAt { get; private set; }

    /// <summary>Set once, so a trip raises at most one hours-shortfall alert.</summary>
    public DateTime? HosShortfallFlaggedAt { get; set; }

    /// <summary>
    /// Set only by the alert job's conditional UPDATE. The save that raised the alert can commit after the job
    /// runs, and a tracked write would overwrite the link.
    /// </summary>
    public Guid? AlertConversationId { get; set; }

    /// <summary>Not linked here: the caller claims <see cref="AlertConversationId"/> for it.</summary>
    public AgentConversation CreateAlertConversation()
    {
        return new AgentConversation
        {
            CreatedById = Guid.Empty,
            Kind = AgentConversationKind.Dispatch,
            Title = $"Trip alert: trip #{Number}"
        };
    }

    public bool IsActive => Status is TripStatus.Dispatched or TripStatus.InTransit;

    public bool IsEtaDue(DateTime now)
    {
        return EtaUpdatedAt is not { } updatedAt || now - updatedAt >= EtaRefreshInterval;
    }

    /// <summary>In route order.</summary>
    public IReadOnlyList<TripStop> GetRemainingStops()
    {
        return [.. Stops.Where(s => s.ArrivedAt is null).OrderBy(s => s.Order)];
    }

    /// <summary>Raises <see cref="TripStopRunningLateEvent"/> at most once per stop.</summary>
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
