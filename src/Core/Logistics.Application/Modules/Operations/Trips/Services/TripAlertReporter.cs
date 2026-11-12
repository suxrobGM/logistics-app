using System.Globalization;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Services;

internal sealed class TripAlertReporter(
    ITenantUnitOfWork tenantUow,
    IEtaService etaService,
    ICommandEnqueuer commandEnqueuer) : ITripAlertReporter
{
    public async Task CheckHosShortfallAsync(Guid employeeId, int drivingMinutesRemaining, CancellationToken ct = default)
    {
        if (await FindActiveTripAsync(employeeId, ct) is { } trip)
        {
            FlagIfShort(trip, drivingMinutesRemaining);
        }
    }

    public async Task CheckHosShortfallAsync(Trip trip, CancellationToken ct = default)
    {
        if (trip.HosShortfallFlaggedAt is not null || trip.DriverId is not { } driverId)
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
        Enqueue(trip, TripAlertKind.HosShortfall,
            $"The driver has {FormatMinutes(drivingMinutesRemaining)} of driving left, but the remaining stops need about " +
            $"{FormatMinutes(minutesNeeded)} of driving. They will run out of hours about " +
            $"{FormatMinutes(minutesNeeded - drivingMinutesRemaining)} short of the last stop.");
    }

    public async Task ReportViolationsAsync(Guid employeeId, int newViolations, CancellationToken ct = default)
    {
        if (newViolations <= 0 || await FindActiveTripAsync(employeeId, ct) is not { } trip)
        {
            return;
        }

        Enqueue(trip, TripAlertKind.HosViolation,
            $"The ELD reported {newViolations} new HOS violation(s) for the driver of this trip.");
    }

    public async Task ReportLateStopAsync(TripStopRunningLateEvent late, CancellationToken ct = default)
    {
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

        Enqueue(trip, TripAlertKind.RunningLate, details);
    }

    public async Task ReportDetentionAsync(Guid tripId, string details, CancellationToken ct = default)
    {
        if (await tenantUow.Repository<Trip>().GetByIdAsync(tripId, ct) is { } trip)
        {
            Enqueue(trip, TripAlertKind.Detention, details);
        }
    }

    /// <summary>The job can start before the caller's save commits, so it finds or opens the conversation itself.</summary>
    private void Enqueue(Trip trip, TripAlertKind kind, string details)
    {
        commandEnqueuer.Enqueue(new RaiseTripAlertCommand
        {
            TenantId = tenantUow.GetCurrentTenant().Id,
            TripId = trip.Id,
            Kind = kind,
            Details = details
        });
    }

    private async Task<Trip?> FindActiveTripAsync(Guid employeeId, CancellationToken ct)
    {
        return await tenantUow.Repository<Trip>().Query()
            .Where(t => t.DriverId == employeeId || t.SecondaryDriverId == employeeId)
            .Where(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit)
            .OrderByDescending(t => t.DispatchedAt)
            .FirstOrDefaultAsync(ct);
    }

    private static string FormatMinutes(int minutes)
    {
        return minutes >= 60 ? $"{minutes / 60}h {minutes % 60:00}m" : $"{minutes}m";
    }
}
