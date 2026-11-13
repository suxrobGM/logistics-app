using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Trucks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class CreateTruckHandler(
    ITenantUnitOfWork tenantUow,
    IVehicleTransportCheck vehicleTransportCheck,
    ITruckDriverFinder driverFinder) : IRequestHandler<CreateTruckCommand, Result>
{

    public async Task<Result> Handle(
        CreateTruckCommand req, CancellationToken ct)
    {
        var limitResult = await CheckTruckLimitAsync(ct);
        if (!limitResult.IsSuccess) return limitResult;

        var typeCheck = await vehicleTransportCheck.CheckTruckTypeAsync(req.TruckType);
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
            return Result.Fail(driverResult);
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
        var plan = tenantUow.GetCurrentTenant().Subscription?.Plan;
        if (plan?.MaxTrucks is null)
        {
            return Result.Ok();
        }

        var currentCount = await tenantUow.Repository<Truck>().Query().CountAsync(ct);
        return plan.GetTruckLimitReason(currentCount + 1) is { } reason
            ? Result.Fail(reason, ErrorCodes.ResourceLimitReached)
            : Result.Ok();
    }
}
