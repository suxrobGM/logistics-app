using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTruckHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<CreateTruckCommand, Result>
{

    public async Task<Result> Handle(
        CreateTruckCommand req, CancellationToken ct)
    {
        var truckWithThisNumber = await tenantUow.Repository<Truck>().GetAsync(i => i.Number == req.TruckNumber, ct);

        if (truckWithThisNumber is not null)
        {
            return Result.Fail($"Already exists truck with number {req.TruckNumber}");
        }

        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.MainDriverId, ct);

        if (driver is null)
        {
            return Result.Fail($"Could not find driver with ID {req.MainDriverId}");
        }

        var alreadyAssociatedTruck = await tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == driver.Id, ct);

        if (alreadyAssociatedTruck is not null)
        {
            return Result.Fail(
                $"Driver '{driver.GetFullName()}' is already associated with the truck number '{req.TruckNumber}'");
        }

        var truckEntity = Truck.Create(req.TruckNumber, req.TruckType, driver);

        if (req.VehicleCapacity.HasValue)
        {
            truckEntity.VehicleCapacity = req.VehicleCapacity.Value;
        }

        await tenantUow.Repository<Truck>().AddAsync(truckEntity, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
