using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Commands;

internal sealed class CreateTruckHandler(
    ITenantUnitOfWork tenantUow,
    ITenantService tenantService,
    IMasterUnitOfWork masterUow) : IAppRequestHandler<CreateTruckCommand, Result>
{

    public async Task<Result> Handle(
        CreateTruckCommand req, CancellationToken ct)
    {
        var limitResult = await CheckTruckLimitAsync(ct);
        if (!limitResult.IsSuccess) return limitResult;

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

        truckEntity.Make = req.Make;
        truckEntity.Model = req.Model;
        truckEntity.Year = req.Year;
        truckEntity.Vin = req.Vin;
        truckEntity.LicensePlate = req.LicensePlate;
        truckEntity.LicensePlateState = req.LicensePlateState;

        await tenantUow.Repository<Truck>().AddAsync(truckEntity, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<Result> CheckTruckLimitAsync(CancellationToken ct)
    {
        var tenant = tenantService.GetCurrentTenant();

        if (tenant.Subscription is null)
        {
            return Result.Ok();
        }

        var maxTrucks = tenant.Subscription.Plan.MaxTrucks;

        if (maxTrucks is null)
        {
            return Result.Ok();
        }

        var currentCount = await tenantUow.Repository<Truck>().Query().CountAsync(ct);

        if (currentCount >= maxTrucks.Value)
        {
            return Result.Fail(
                $"Your {tenant.Subscription.Plan.Name} plan allows a maximum of {maxTrucks.Value} trucks. Please upgrade your plan to add more.",
                ErrorCodes.ResourceLimitReached);
        }

        return Result.Ok();
    }
}
