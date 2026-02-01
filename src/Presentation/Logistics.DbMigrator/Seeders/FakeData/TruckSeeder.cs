using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds trucks assigned to drivers.
/// </summary>
internal class TruckSeeder(ILogger<TruckSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TruckSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 130;
    public override IReadOnlyList<string> DependsOn => [nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Truck>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var drivers = employees.Drivers;

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available to assign trucks to");
            context.CreatedTrucks = [];
            LogCompleted(0);
            return;
        }

        var trucksList = new List<Truck>();
        var truckNumber = 101;
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        foreach (var driver in drivers)
        {
            var truckType = random.Pick([TruckType.CarHauler, TruckType.FreightTruck]);
            var truck = Truck.Create(truckNumber.ToString(), truckType, driver);
            truck.VehicleCapacity = truckType == TruckType.CarHauler ? 7 : 0;

            truckNumber++;
            trucksList.Add(truck);
            await truckRepository.AddAsync(truck, cancellationToken);
            logger.LogInformation("Created truck {Number} of type {Type}", truck.Number, truck.Type);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedTrucks = trucksList;
        LogCompleted(trucksList.Count);
    }
}
