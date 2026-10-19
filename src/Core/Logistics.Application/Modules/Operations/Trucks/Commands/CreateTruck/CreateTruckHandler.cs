using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Trucks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class CreateTruckHandler(
    ITenantUnitOfWork tenantUow,
    IVehicleTransportGuard vehicleTransportGuard,
    ITruckDriverFinder driverFinder) : IRequestHandler<CreateTruckCommand, Result>
{

    public async Task<Result> Handle(
        CreateTruckCommand req, CancellationToken ct)
    {
        var limitResult = await CheckTruckLimitAsync(ct);
        if (!limitResult.IsSuccess) return limitResult;

        var typeCheck = await vehicleTransportGuard.CheckTruckTypeAsync(req.TruckType);
        if (!typeCheck.IsSuccess) return typeCheck;

        var number = req.TruckNumber.Trim();
        var truckWithThisNumber = await tenantUow.Repository<Truck>()
            .GetAsync(i => i.Number.ToLower() == number.ToLower(), ct);

        if (truckWithThisNumber is not null)
        {
            return Result.Fail($"Already exists truck with number {number}");
        }

        var driverResult = await driverFinder.FindAssignableDriverAsync(req.MainDriverId, null, ct);
        if (driverResult.Value is not { } driver)
        {
            return Result.Fail(driverResult.Error!);
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

        if (truckEntity.Vin is not null && await tenantUow.Repository<Truck>().GetAsync(i => i.Vin == truckEntity.Vin, ct) is not null)
        {
            return Result.Fail($"Already exists truck with VIN {truckEntity.Vin}");
        }

        await tenantUow.Repository<Truck>().AddAsync(truckEntity, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<Result> CheckTruckLimitAsync(CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

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
