using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetTruckGeolocationHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<SetTruckGeolocationHandler> logger)
    : IAppRequestHandler<SetTruckGeolocationCommand, Result>
{
    public async Task<Result> Handle(
        SetTruckGeolocationCommand req, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(req.GeolocationData.TenantId);
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.GeolocationData.TruckId, ct);

        if (truck is null)
        {
            logger.LogWarning("Could not find a truck with ID {TruckId}, skipped saving geolocation data",
                req.GeolocationData.TruckId);
            return Result.Ok();
        }

        truck.CurrentAddress = req.GeolocationData.CurrentAddress;
        truck.CurrentLocation = req.GeolocationData.CurrentLocation;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
