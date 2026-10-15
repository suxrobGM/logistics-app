using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class UpdateTruckHandler(
    ITenantUnitOfWork tenantUow,
    IVehicleTransportGuard vehicleTransportGuard) : IRequestHandler<UpdateTruckCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTruckCommand req, CancellationToken ct)
    {
        var truckRepository = tenantUow.Repository<Truck>();
        var truck = await truckRepository.GetByIdAsync(req.Id, ct);

        if (truck is null)
        {
            return Result.Fail($"Could not find a truck with ID {req.Id}");
        }

        // Only a change is checked, so existing car haulers stay editable after the feature is turned off.
        var typeCheck =
            await vehicleTransportGuard.CheckTruckTypeAsync(req.TruckType == truck.Type ? null : req.TruckType);
        if (!typeCheck.IsSuccess)
        {
            return typeCheck;
        }

        var numberTaken = truckRepository.Query().Any(i => i.Number == req.TruckNumber &&
                                                           i.Id != truck.Id);
        if (numberTaken)
        {
            return Result.Fail($"Already exists truck with number {req.TruckNumber}");
        }

        // Update drivers
        if (await SetDriverAsync(truck, req.MainDriverId, true) is { } fail1)
        {
            return fail1;
        }

        if (await SetDriverAsync(truck, req.SecondaryDriverId, false) is { } fail2)
        {
            return fail2;
        }

        if (await SetTrailerAsync(truck, req.CurrentTrailerId, ct) is { } trailerFail)
        {
            return trailerFail;
        }

        truck.Number = PropertyUpdater.UpdateIfChanged(req.TruckNumber, truck.Number);
        truck.Type = PropertyUpdater.UpdateIfChanged(req.TruckType, truck.Type);
        truck.Status = PropertyUpdater.UpdateIfChanged(req.TruckStatus, truck.Status);
        truck.VehicleCapacity = PropertyUpdater.UpdateIfChanged(req.VehicleCapacity, truck.VehicleCapacity);
        truck.Make = PropertyUpdater.UpdateIfChanged(req.Make, truck.Make);
        truck.Model = PropertyUpdater.UpdateIfChanged(req.Model, truck.Model);

        truck.Vin = PropertyUpdater.UpdateIfChanged(req.Vin, truck.Vin);
        truck.LicensePlate = PropertyUpdater.UpdateIfChanged(req.LicensePlate, truck.LicensePlate);
        truck.LicensePlateState = PropertyUpdater.UpdateIfChanged(req.LicensePlateState, truck.LicensePlateState);

        if (req.Year is not null && truck.Year != req.Year)
        {
            truck.Year = req.Year.Value;
        }

        if (req.AdrEquipment is not null)
        {
            truck.AdrEquipment = req.AdrEquipment.ToDomain();
        }

        if (req.IsHazmatPlacarded.HasValue)
        {
            truck.IsHazmatPlacarded = req.IsHazmatPlacarded.Value;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>
    ///     Hooks (or unhooks) a trailer. A trailer hooked to another truck is unhooked there first,
    ///     since the unique index allows one truck per trailer.
    /// </summary>
    private async Task<Result?> SetTrailerAsync(Truck truck, Guid? trailerId, CancellationToken ct)
    {
        if (trailerId == truck.CurrentTrailerId)
        {
            return null;
        }

        if (trailerId is null)
        {
            truck.CurrentTrailer = null;
            truck.CurrentTrailerId = null;
            return null;
        }

        var trailer = await tenantUow.Repository<Trailer>().GetByIdAsync(trailerId.Value, ct);
        if (trailer is null)
        {
            return Result.Fail($"Could not find a trailer with ID {trailerId}");
        }

        if (trailer.Status != TrailerStatus.Active)
        {
            return Result.Fail($"Trailer '{trailer.Number}' is {trailer.Status.GetDescription()} and cannot be hooked");
        }

        var previousTruck = await tenantUow.Repository<Truck>()
            .GetAsync(i => i.CurrentTrailerId == trailer.Id && i.Id != truck.Id, ct);
        if (previousTruck is not null)
        {
            previousTruck.CurrentTrailer = null;
            previousTruck.CurrentTrailerId = null;

            // Postgres checks the unique index per statement, so the unhook must land before the hook.
            await tenantUow.SaveChangesAsync(ct);
        }

        truck.CurrentTrailer = trailer;
        truck.CurrentTrailerId = trailer.Id;
        return null;
    }

    /// <summary>
    ///     Assigns (or clears) a driver and returns a failure <see cref="Result" />
    ///     if the supplied ID doesn’t exist.  Returns <c>null</c> on success.
    /// </summary>
    private async Task<Result?> SetDriverAsync(Truck truck, Guid? newDriverId, bool isMain)
    {
        var currentId = isMain ? truck.MainDriverId : truck.SecondaryDriverId;
        if (newDriverId == currentId) // nothing to change
        {
            return null;
        }

        if (newDriverId is null)
        {
            if (isMain)
            {
                truck.MainDriver = null;
            }
            else
            {
                truck.SecondaryDriver = null;
            }

            return null;
        }

        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(newDriverId.Value);
        if (driver is null)
        {
            return Result.Fail($"Could not find a driver with ID {newDriverId}");
        }

        if (isMain)
        {
            truck.MainDriver = driver;
        }
        else
        {
            truck.SecondaryDriver = driver;
        }

        return null;
    }
}
