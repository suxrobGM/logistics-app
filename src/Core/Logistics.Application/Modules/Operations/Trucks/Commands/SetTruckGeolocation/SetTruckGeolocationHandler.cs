using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class SetTruckGeolocationHandler(
    ITenantUnitOfWork tenantUow,
    ITruckLocationRecorder locationRecorder,
    ILogger<SetTruckGeolocationHandler> logger)
    : IRequestHandler<SetTruckGeolocationCommand, Result>
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

        var geolocation = req.GeolocationData;
        if (geolocation.CurrentLocation is not null)
        {
            await locationRecorder.RecordAsync(
                truck, geolocation.CurrentLocation, DateTime.UtcNow, address: geolocation.CurrentAddress, ct: ct);
        }
        else
        {
            truck.CurrentAddress = geolocation.CurrentAddress;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
