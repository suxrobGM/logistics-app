using System.Globalization;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.AIDispatch.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Services;

internal sealed class TripAlertReporter(
    ITenantUnitOfWork tenantUow,
    IEtaService etaService,
    ICommandEnqueuer commandEnqueuer,
    IFeatureService featureService) : ITripAlertReporter
{
    public async Task CheckHosShortfallsAsync(
        IReadOnlyDictionary<Guid, int> drivingMinutesByDriver, CancellationToken ct = default)
    {
        if (drivingMinutesByDriver.Count == 0 || !await IsEnabledAsync())
        {
            return;
        }

        List<Guid?> driverIds = [.. drivingMinutesByDriver.Keys.Select(id => (Guid?)id)];
        var trips = await QueryActiveTrips()
            .Where(t => t.HosShortfallFlaggedAt == null)
            .Where(t => driverIds.Contains(t.DriverId) || driverIds.Contains(t.SecondaryDriverId))
            .ToListAsync(ct);

        foreach (var (driverId, drivingMinutesRemaining) in drivingMinutesByDriver)
        {
            var trip = trips.FirstOrDefault(t => t.DriverId == driverId || t.SecondaryDriverId == driverId);
            if (trip is not null)
            {
                FlagIfShort(trip, drivingMinutesRemaining);
            }
        }
    }

    public async Task CheckHosShortfallAsync(Trip trip, CancellationToken ct = default)
    {
        if (trip.HosShortfallFlaggedAt is not null || trip.DriverId is not { } driverId)
        {
            return;
        }

        if (!await IsEnabledAsync())
        {
            return;
        }

        var hos = await tenantUow.Repository<DriverHosStatus>().GetAsync(s => s.EmployeeId == driverId, ct);
        if (hos is not null)
        {
            FlagIfShort(trip, hos.DrivingMinutesRemaining);
        }
    }

    private void FlagIfShort(Trip trip, int drivingMinutesRemaining)
    {
        if (trip.HosShortfallFlaggedAt is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (etaService.GetRemainingDriveTime(trip, now) is not { } driveTimeLeft)
        {
            return;
        }

        var minutesNeeded = (int)Math.Ceiling(driveTimeLeft.TotalMinutes);
        if (drivingMinutesRemaining >= minutesNeeded)
        {
            return;
        }

        trip.HosShortfallFlaggedAt = now;
        Enqueue(trip.Id, TripAlertKind.HosShortfall,
            $"The driver has {FormatMinutes(drivingMinutesRemaining)} of driving left, but the remaining stops need about " +
            $"{FormatMinutes(minutesNeeded)} of driving. They will run out of hours about " +
            $"{FormatMinutes(minutesNeeded - drivingMinutesRemaining)} short of the last stop.");
    }

    public async Task ReportViolationsAsync(Guid employeeId, int newViolations, CancellationToken ct = default)
    {
        if (newViolations <= 0 || !await IsEnabledAsync())
        {
            return;
        }

        if (await FindActiveTripAsync(employeeId, ct) is not { } trip)
        {
            return;
        }

        Enqueue(trip.Id, TripAlertKind.HosViolation,
            $"The ELD reported {newViolations} new HOS violation(s) for the driver of this trip.");
    }

    public async Task ReportLateStopAsync(TripStopRunningLateEvent late, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync())
        {
            return;
        }

        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(late.TripId, ct);
        if (trip?.Stops.FirstOrDefault(s => s.Id == late.StopId) is not { } stop)
        {
            return;
        }

        var place = stop.Type == TripStopType.PickUp ? "pickup" : "delivery";
        var lateMinutes = (int)(late.EstimatedArrivalAt - late.AppointmentEnd).TotalMinutes;
        var details = string.Create(CultureInfo.InvariantCulture,
            $"The {place} in {stop.Address.City} for load #{stop.Load.Number} (load_id {late.LoadId}) is now " +
            $"expected at {late.EstimatedArrivalAt:yyyy-MM-dd HH:mm} UTC, {FormatMinutes(lateMinutes)} after its " +
            $"appointment window ends at {late.AppointmentEnd:yyyy-MM-dd HH:mm} UTC.");

        Enqueue(trip.Id, TripAlertKind.RunningLate, details);
    }

    public async Task ReportDetentionAsync(Guid tripId, string details, CancellationToken ct = default)
    {
        if (await IsEnabledAsync())
        {
            Enqueue(tripId, TripAlertKind.Detention, details);
        }
    }

    private Task<bool> IsEnabledAsync() =>
        featureService.IsFeatureEnabledAsync(tenantUow.GetCurrentTenant().Id, TenantFeature.AITripAlerts);

    /// <summary>The job can start before the caller's save commits, so it finds or opens the conversation itself.</summary>
    private void Enqueue(Guid tripId, TripAlertKind kind, string details)
    {
        commandEnqueuer.Enqueue(new RaiseTripAlertCommand
        {
            TenantId = tenantUow.GetCurrentTenant().Id,
            TripId = tripId,
            Kind = kind,
            Details = details
        });
    }

    private async Task<Trip?> FindActiveTripAsync(Guid employeeId, CancellationToken ct)
    {
        return await QueryActiveTrips()
            .Where(t => t.DriverId == employeeId || t.SecondaryDriverId == employeeId)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>Newest first, so a driver's most recently dispatched trip wins.</summary>
    private IQueryable<Trip> QueryActiveTrips()
    {
        return tenantUow.Repository<Trip>().Query()
            .Where(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit)
            .OrderByDescending(t => t.DispatchedAt);
    }

    private static string FormatMinutes(int minutes)
    {
        return minutes >= 60 ? $"{minutes / 60}h {minutes % 60:00}m" : $"{minutes}m";
    }
}
