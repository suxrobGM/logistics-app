using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds general freight loads.
/// </summary>
internal class LoadSeeder(ILogger<LoadSeeder> logger) : SeederBase(logger)
{
    private readonly Random _random = new();
    private readonly DateTime _startDate = DateTime.UtcNow.AddMonths(-2);
    private readonly DateTime _endDate = DateTime.UtcNow.AddDays(-1);

    public override string Name => nameof(LoadSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 140;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(EmployeeSeeder), nameof(TruckSeeder), nameof(CustomerSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Load>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var trucks = context.CreatedTrucks ?? throw new InvalidOperationException("Trucks not seeded");
        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");

        var dryVanTrucks = trucks.Where(i => i.Type == TruckType.FreightTruck).ToList();

        if (dryVanTrucks.Count == 0)
        {
            Logger.LogWarning("No freight trucks available for loads");
            LogCompleted(0);
            return;
        }

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var count = 0;

        for (long i = 1; i <= 100; i++)
        {
            var truck = _random.Pick(dryVanTrucks);
            var customer = _random.Pick(customers);
            var dispatcher = _random.Pick(employees.Dispatchers);
            var origin = _random.Pick(RoutePoints.Points);
            var dest = _random.Pick(RoutePoints.Points.Where(p => p != origin).ToArray());

            var load = BuildLoad(i, origin, dest, LoadType.GeneralFreight, truck, dispatcher, customer);
            await loadRepository.AddAsync(load, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private Load BuildLoad(
        long seq,
        (Address Address, double Longitude, double Latitude) origin,
        (Address Address, double Longitude, double Latitude) dest,
        LoadType type,
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
            $"Freight Load {seq}",
            type,
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
