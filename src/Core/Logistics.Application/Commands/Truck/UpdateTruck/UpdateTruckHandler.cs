using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTruckHandler : RequestHandler<UpdateTruckCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateTruckCommand req, CancellationToken ct)
    {
        var truckRepository = _tenantUow.Repository<Truck>();
        var truck = await truckRepository.GetByIdAsync(req.Id);

        if (truck is null) return Result.Fail($"Could not find a truck with ID {req.Id}");

        var numberTaken = truckRepository.Query().Any(i => i.Number == req.TruckNumber &&
                                                           i.Id != truck.Id);
        if (numberTaken) return Result.Fail($"Already exists truck with number {req.TruckNumber}");

        // Update drivers
        if (await SetDriverAsync(truck, req.MainDriverId, true) is { } fail1) return fail1;
        if (await SetDriverAsync(truck, req.SecondaryDriverId, false) is { } fail2) return fail2;

        truck.Number = PropertyUpdater.UpdateIfChanged(req.TruckNumber, truck.Number);
        truck.Type = PropertyUpdater.UpdateIfChanged(req.TruckType, truck.Type);
        truck.Status = PropertyUpdater.UpdateIfChanged(req.TruckStatus, truck.Status);

        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }

    /// <summary>
    ///     Assigns (or clears) a driver and returns a failure <see cref="Result" />
    ///     if the supplied ID doesn’t exist.  Returns <c>null</c> on success.
    /// </summary>
    private async Task<Result?> SetDriverAsync(Truck truck, Guid? newDriverId, bool isMain)
    {
        var currentId = isMain ? truck.MainDriverId : truck.SecondaryDriverId;
        if (newDriverId == currentId) // nothing to change
            return null;

        if (newDriverId is null)
        {
            if (isMain)
                truck.MainDriver = null;
            else
                truck.SecondaryDriver = null;
            return null;
        }

        var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(newDriverId.Value);
        if (driver is null)
            return Result.Fail($"Could not find a driver with ID {newDriverId}");

        if (isMain)
            truck.MainDriver = driver;
        else
            truck.SecondaryDriver = driver;

        return null;
    }
}
