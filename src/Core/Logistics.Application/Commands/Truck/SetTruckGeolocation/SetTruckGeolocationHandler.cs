using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetTruckGeolocationHandler : IAppRequestHandler<SetTruckGeolocationCommand, Result>
{
    private readonly ILogger<SetTruckGeolocationHandler> _logger;
    private readonly ITenantUnitOfWork _tenantUow;

    public SetTruckGeolocationHandler(
        ITenantUnitOfWork tenantUow,
        ILogger<SetTruckGeolocationHandler> logger)
    {
        _logger = logger;
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        SetTruckGeolocationCommand req, CancellationToken ct)
    {
        await _tenantUow.SetCurrentTenantByIdAsync(req.GeolocationData.TenantId);
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.GeolocationData.TruckId, ct);

        if (truck is null)
        {
            _logger.LogWarning("Could not find a truck with ID {TruckId}, skipped saving geolocation data",
                req.GeolocationData.TruckId);
            return Result.Ok();
        }

        truck.CurrentAddress = req.GeolocationData.CurrentAddress;
        truck.CurrentLocation = req.GeolocationData.CurrentLocation;
        await _tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
