using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class SetTruckGeolocationHandler(
    ITenantUnitOfWork tenantUow,
    ITruckLocationRecorder locationRecorder,
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

        if (req.GeolocationData.CurrentLocation is not null)
        {
            // Appends the breadcrumb, accrues IFTA jurisdiction mileage, sets CurrentLocation
            await locationRecorder.RecordAsync(truck, req.GeolocationData.CurrentLocation, DateTime.UtcNow, ct: ct);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
