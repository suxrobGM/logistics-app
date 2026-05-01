using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds trucks assigned to drivers using region-specific makes, models, VIN WMIs and license plates.
/// Mixes FreightTruck / CarHauler / ContainerTruck types so the demo exercises all dispatch paths.
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
        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");
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
        var truckRepo = context.TenantUnitOfWork.Repository<Truck>();

        for (var idx = 0; idx < drivers.Count; idx++)
        {
            var driver = drivers[idx];
            var truckType = PickTruckType(idx, drivers.Count);
            var truck = Truck.Create(truckNumber.ToString(), truckType, driver);

            truck.VehicleCapacity = truckType == TruckType.CarHauler ? 7 : 0;

            var makeModel = truckType switch
            {
                TruckType.CarHauler => random.Pick((IList<TruckMakeModel>)region.CarHaulerModels),
                TruckType.ContainerTruck => random.Pick((IList<TruckMakeModel>)region.ContainerTruckModels),
                _ => random.Pick((IList<TruckMakeModel>)region.FreightTruckModels)
            };
            truck.Make = makeModel.Make;
            truck.Model = makeModel.Model;
            truck.Year = random.Next(2018, 2025);
            truck.Vin = region.GenerateVin(makeModel.Make);

            var plate = region.GeneratePlate();
            truck.LicensePlate = plate.Number;
            truck.LicensePlateState = plate.RegionCode;

            truckNumber++;
            trucksList.Add(truck);
            await truckRepo.AddAsync(truck, cancellationToken);
            logger.LogInformation("Created truck {Number} ({Year} {Make} {Model})",
                truck.Number, truck.Year, truck.Make, truck.Model);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedTrucks = trucksList;
        LogCompleted(trucksList.Count);
    }

    /// <summary>
    /// Distributes truck types so the demo has a meaningful mix:
    /// the first driver gets a CarHauler (anchors the trip seeder),
    /// the second gets a ContainerTruck (anchors intermodal loads),
    /// the rest are FreightTrucks. With 5 drivers: 1 CarHauler + 1 ContainerTruck + 3 Freight.
    /// </summary>
    private static TruckType PickTruckType(int driverIdx, int driverCount)
    {
        if (driverIdx == 0) return TruckType.CarHauler;
        if (driverIdx == 1 && driverCount > 1) return TruckType.ContainerTruck;
        return TruckType.FreightTruck;
    }
}
