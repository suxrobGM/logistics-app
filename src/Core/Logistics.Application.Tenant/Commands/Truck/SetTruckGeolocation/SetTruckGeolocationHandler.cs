using Microsoft.Extensions.Logging;

namespace Logistics.Application.Tenant.Commands;

internal sealed class SetTruckGeolocationHandler : RequestHandler<SetTruckGeolocationCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<SetTruckGeolocationHandler> _logger;

    public SetTruckGeolocationHandler(
        ITenantRepository tenantRepository,
        ILogger<SetTruckGeolocationHandler> logger)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        SetTruckGeolocationCommand req, CancellationToken cancellationToken)
    {
        if (req.GeolocationData is null)
            return ResponseResult.CreateSuccess();
        
        _tenantRepository.SetTenantId(req.GeolocationData.TenantId);
        var truck = await _tenantRepository.GetAsync<Truck>(req.GeolocationData.TruckId);

        if (truck is null)
        {
            _logger.LogWarning("Could not find a truck with ID {TruckId}, skipped saving geolocation data", req.GeolocationData.TruckId);
            return ResponseResult.CreateSuccess();
        }

        truck.LastKnownLocation = req.GeolocationData.CurrentAddress;
        truck.LastKnownLocationLat = req.GeolocationData.Latitude;
        truck.LastKnownLocationLong = req.GeolocationData.Longitude;
        _tenantRepository.Update(truck);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
