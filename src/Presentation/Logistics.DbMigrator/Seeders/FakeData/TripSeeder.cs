using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds car hauler trips with loads.
/// </summary>
internal class TripSeeder(ILogger<TripSeeder> logger) : SeederBase(logger)
{
    private readonly Random _random = new();
    private readonly DateTime _startDate = DateTime.UtcNow.AddMonths(-2);
    private readonly DateTime _endDate = DateTime.UtcNow.AddDays(-1);

    public override string Name => nameof(TripSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 150;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder), nameof(CustomerSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Trip>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var trucks = context.CreatedTrucks ?? throw new InvalidOperationException("Trucks not seeded");
        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");

        var carHaulerTrucks = trucks.Where(t => t.Type == TruckType.CarHauler).ToList();

        if (carHaulerTrucks.Count == 0)
        {
            Logger.LogWarning("No car hauler trucks available for trips");
            LogCompleted(0);
            return;
        }

        var tripRepo = context.TenantUnitOfWork.Repository<Trip>();
        var loadRepo = context.TenantUnitOfWork.Repository<Load>();
        var count = 0;

        for (var tripIdx = 0; tripIdx < 30; tripIdx++)
        {
            var truck = _random.Pick(carHaulerTrucks);
            var dispatcher = _random.Pick(employees.Dispatchers);
            var customer = _random.Pick(customers);

            var loadsCount = _random.Next(2, 5);
            var maxStart = RoutePoints.Points.Length - (loadsCount + 1);
            var startIndex = _random.Next(0, maxStart + 1);
            var loads = new List<Load>();

            for (var leg = 0; leg < loadsCount; leg++)
            {
                var origin = RoutePoints.Points[startIndex + leg];
                var dest = RoutePoints.Points[startIndex + leg + 1];
                var load = BuildLoad(tripIdx * 10 + leg + 1, origin, dest, truck, dispatcher, customer);

                loads.Add(load);
                await loadRepo.AddAsync(load, cancellationToken);
            }

            var trip = Trip.Create($"Trip {tripIdx + 1}", truck, loads);
            trip.Dispatch();

            foreach (var stop in trip.Stops)
            {
                trip.MarkStopArrived(stop.Id);
            }

            await tripRepo.AddAsync(trip, cancellationToken);
            count++;
            Logger.LogInformation("Created Trip {Trip} with {LoadCount} loads", trip.Name, loadsCount);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private Load BuildLoad(
        long seq,
        (Address Address, double Longitude, double Latitude) origin,
        (Address Address, double Longitude, double Latitude) dest,
        Truck truck,
        Employee dispatcher,
        Customer customer)
    {
        var dispatchedAt = _random.UtcDate(_startDate, _endDate);
        var pickedUpAt = dispatchedAt.AddHours(_random.Next(1, 12));
        var deliveredAt = pickedUpAt.AddHours(_random.Next(4, 48));
        var deliveryCost = _random.Next(1_000, 3_000);
        var originLocation = new GeoPoint(origin.Longitude, origin.Latitude);
        var destLocation = new GeoPoint(dest.Longitude, dest.Latitude);

        var load = Load.Create(
            $"Car Load {seq}",
            LoadType.Vehicle,
            deliveryCost,
            origin.Address,
            originLocation,
            dest.Address,
            destLocation,
            customer,
            truck,
            dispatcher);

        load.Dispatch(dispatchedAt);
        load.ConfirmPickup(pickedUpAt);
        load.ConfirmDelivery(deliveredAt);

        return load;
    }
}
