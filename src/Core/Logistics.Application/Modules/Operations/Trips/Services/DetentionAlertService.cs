using Logistics.Application.Abstractions.Notifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Services;

internal sealed class DetentionAlertService(
    ITenantUnitOfWork tenantUow,
    INotificationService notificationService) : IDetentionAlertService
{
    public async Task<int> SendAlertsAsync(DateTime now, CancellationToken ct = default)
    {
        var waiting = await tenantUow.Repository<TripStop>().Query()
            .Where(s => s.ArrivedAt != null && s.DepartedAt == null && s.DetentionAlertedAt == null)
            .Select(s => new { Stop = s, s.Load.Number, s.Load.Customer, TruckNumber = s.Trip.Truck!.Number })
            .ToListAsync(ct);

        var tenantSettings = tenantUow.GetCurrentTenant().Settings;
        var sent = 0;
        foreach (var row in waiting)
        {
            var detention = DetentionCalculator.Calculate(row.Stop, now, DetentionTerms.For(tenantSettings, row.Customer));
            if (detention is not { IsOverFreeTime: true })
            {
                continue;
            }

            var (title, message) = BuildAlert(row.Stop.Type, row.Number, row.TruckNumber, detention);
            await notificationService.SendNotificationAsync(title, message);
            row.Stop.DetentionAlertedAt = now;
            sent++;
        }

        if (sent > 0)
        {
            await tenantUow.SaveChangesAsync(ct);
        }

        return sent;
    }

    internal static (string Title, string Message) BuildAlert(
        TripStopType stopType, long loadNumber, string? truckNumber, Detention detention)
    {
        var place = stopType == TripStopType.PickUp ? "pickup" : "delivery";
        var truck = truckNumber is null ? "The truck" : $"Truck {truckNumber}";
        var dwell = $"{detention.DwellMinutes / 60}h {detention.DwellMinutes % 60:00}m";
        var billing = detention.RatePerHour is { } rate
            ? $" Detention accrues at {rate:0.00}/h; it is billed when the truck departs."
            : "";

        return (
            $"Load #{loadNumber}: detention started at {place}",
            $"{truck} has been at the {place} for {dwell}, past the {detention.FreeMinutes} minutes of free time.{billing}");
    }
}
