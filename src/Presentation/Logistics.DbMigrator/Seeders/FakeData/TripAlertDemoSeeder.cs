using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// An in-transit trip whose driver is short on hours and which will miss its delivery window. The first truck
/// position after seeding fires the shortfall and late alerts.
/// </summary>
internal class TripAlertDemoSeeder(ILogger<TripAlertDemoSeeder> logger) : SeederBase(logger)
{
    /// <summary>Straight-line metres to drive minutes, roughly matching the ETA fallback.</summary>
    private const double MinutesPerMeter = 1.25 / (80_000 / 60.0);

    /// <summary>Under the 11-hour US driving limit, so the seeded clock stays believable.</summary>
    private const int MaxDrivingMinutes = 600;

    public override string Name => nameof(TripAlertDemoSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 165;
    public override IReadOnlyList<string> DependsOn => [nameof(TripSeeder), nameof(DriverHosStatusSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Trip>().Query()
            .AnyAsync(t => t.Name.StartsWith("Trip alert demo"), cancellationToken);
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var uow = context.TenantUnitOfWork;

        var lane = await uow.Repository<Load>().Query()
            .Where(l => l.Status == LoadStatus.Delivered && l.Type == LoadType.GeneralFreight)
            .OrderByDescending(l => l.Distance)
            .FirstOrDefaultAsync(cancellationToken);
        var truck = await FindIdleTruckAsync(uow, cancellationToken);

        if (lane?.Customer is null || truck?.MainDriverId is not { } driverId)
        {
            Logger.LogWarning("No delivered lane or idle truck with a driver; skipping the trip alert demo");
            LogCompleted(0);
            return;
        }

        var load = Load.Create(
            $"Trip alert demo - {lane.OriginAddress.City} to {lane.DestinationAddress.City}",
            LoadType.GeneralFreight, lane.DeliveryCost, lane.OriginAddress, lane.OriginLocation,
            lane.DestinationAddress, lane.DestinationLocation, lane.Customer, truck, lane.AssignedDispatcher!);
        await uow.Repository<Load>().AddAsync(load, cancellationToken);

        var trip = Trip.Create($"Trip alert demo #{truck.Number}", truck, [load]);
        trip.Dispatch();
        trip.MarkStopArrived(trip.Stops.Single(s => s.Type == TripStopType.PickUp).Id);
        await uow.Repository<Trip>().AddAsync(trip, cancellationToken);

        var driveMinutes = Math.Max(60, (int)(lane.OriginLocation.DistanceTo(lane.DestinationLocation) * MinutesPerMeter));
        var hoursLeft = Math.Min(driveMinutes / 2, MaxDrivingMinutes);
        load.DeliveryAppointmentFrom = DateTime.UtcNow.AddMinutes(driveMinutes / 2.0);
        load.DeliveryAppointmentTo = load.DeliveryAppointmentFrom.Value.AddHours(2);
        truck.CurrentLocation = lane.OriginLocation;
        truck.CurrentAddress = lane.OriginAddress;

        var hos = await uow.Repository<DriverHosStatus>().GetAsync(s => s.EmployeeId == driverId, cancellationToken);
        if (hos is not null)
        {
            hos.CurrentDutyStatus = DutyStatus.Driving;
            hos.DrivingMinutesRemaining = hoursLeft;
            hos.IsInViolation = false;
            hos.LastUpdatedAt = DateTime.UtcNow;
        }

        await uow.SaveChangesAsync(cancellationToken);
        Logger.LogInformation(
            "Trip alert demo: truck {Truck} needs ~{Needed} min of driving, driver has {Left} min. Send one truck position to trigger it",
            truck.Number, driveMinutes, hoursLeft);
        LogCompleted(1);
    }

    private static async Task<Truck?> FindIdleTruckAsync(ITenantUnitOfWork uow, CancellationToken ct)
    {
        var busyTruckIds = await uow.Repository<Trip>().Query()
            .Where(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit)
            .Select(t => t.TruckId)
            .ToListAsync(ct);

        return await uow.Repository<Truck>().Query()
            .Where(t => t.MainDriverId != null && t.Type == TruckType.FreightTruck && !busyTruckIds.Contains(t.Id))
            .FirstOrDefaultAsync(ct);
    }
}
