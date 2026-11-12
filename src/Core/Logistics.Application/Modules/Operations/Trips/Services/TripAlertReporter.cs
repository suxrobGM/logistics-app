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
            await FlagIfShortAsync(trip, drivingMinutesRemaining, ct);
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
            await FlagIfShortAsync(trip, hos.DrivingMinutesRemaining, ct);
        }
    }

    private async Task FlagIfShortAsync(Trip trip, int drivingMinutesRemaining, CancellationToken ct)
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
        await EnqueueAsync(trip, TripAlertKind.HosShortfall,
            $"The driver has {FormatMinutes(drivingMinutesRemaining)} of driving left, but the remaining stops need about " +
            $"{FormatMinutes(minutesNeeded)} of driving. They will run out of hours about " +
            $"{FormatMinutes(minutesNeeded - drivingMinutesRemaining)} short of the last stop.", ct);
    }

    public async Task ReportViolationsAsync(Guid employeeId, int newViolations, CancellationToken ct = default)
    {
        if (newViolations <= 0 || await FindActiveTripAsync(employeeId, ct) is not { } trip)
        {
            return;
        }

        await EnqueueAsync(trip, TripAlertKind.HosViolation,
            $"The ELD reported {newViolations} new HOS violation(s) for the driver of this trip.", ct);
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

        await EnqueueAsync(trip, TripAlertKind.RunningLate, details, ct);
    }

    public async Task ReportDetentionAsync(Guid tripId, string details, CancellationToken ct = default)
    {
        if (await tenantUow.Repository<Trip>().GetByIdAsync(tripId, ct) is { } trip)
        {
            await EnqueueAsync(trip, TripAlertKind.Detention, details, ct);
        }
    }

    /// <summary>
    /// Opens the trip's alert conversation in the caller's save, before the job runs, so alerts raised
    /// together share one conversation and the second waits for the first turn instead of racing it.
    /// </summary>
    private async Task EnqueueAsync(Trip trip, TripAlertKind kind, string details, CancellationToken ct)
    {
        if (trip.AlertConversationId is null)
        {
            await tenantUow.Repository<AgentConversation>().AddAsync(trip.StartAlertConversation(), ct);
        }

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
