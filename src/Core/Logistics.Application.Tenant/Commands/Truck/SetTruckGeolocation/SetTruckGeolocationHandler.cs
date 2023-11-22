using Logistics.Application.Tenant.Mappers;
using Logistics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Tenant.Commands;

internal sealed class SetTruckGeolocationHandler : RequestHandler<SetTruckGeolocationCommand, ResponseResult>
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

    protected override async Task<ResponseResult> HandleValidated(
        SetTruckGeolocationCommand req, CancellationToken cancellationToken)
    {
        _tenantUow.SetCurrentTenantById(req.GeolocationData.TenantId);
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.GeolocationData.TruckId);

        if (truck is null)
        {
            _logger.LogWarning("Could not find a truck with ID {TruckId}, skipped saving geolocation data", req.GeolocationData.TruckId);
            return ResponseResult.CreateSuccess();
        }

        truck.CurrentLocation = req.GeolocationData.CurrentAddress?.ToEntity() ?? Address.NullAddress;
        truck.CurrentLocationLat = req.GeolocationData.Latitude;
        truck.CurrentLocationLong = req.GeolocationData.Longitude;
        _tenantUow.Repository<Truck>().Update(truck);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
