using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetTruckGeolocationHandler : RequestHandler<SetTruckGeolocationCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ILogger<SetTruckGeolocationHandler> _logger;

    public SetTruckGeolocationHandler(
        ITenantUnityOfWork tenantUow,
        ILogger<SetTruckGeolocationHandler> logger)
    {
        _logger = logger;
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        SetTruckGeolocationCommand req, CancellationToken cancellationToken)
    {
        _tenantUow.SetCurrentTenantById(req.GeolocationData.TenantId.ToString());
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.GeolocationData.TruckId);

        if (truck is null)
        {
            _logger.LogWarning("Could not find a truck with ID {TruckId}, skipped saving geolocation data", req.GeolocationData.TruckId);
            return Result.Succeed();
        }

        truck.CurrentAddress = req.GeolocationData.CurrentAddress;
        truck.CurrentLocation = req.GeolocationData.CurrentLocation;
        _tenantUow.Repository<Truck>().Update(truck);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
