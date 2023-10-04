using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadProximityHandler : RequestHandler<UpdateLoadProximityCommand, ResponseResult>
{
    private readonly IPushNotification _pushNotification;
    private readonly ITenantRepository _tenantRepository;

    public UpdateLoadProximityHandler(
        ITenantRepository tenantRepository,
        IPushNotification pushNotification)
    {
        _tenantRepository = tenantRepository;
        _pushNotification = pushNotification;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateLoadProximityCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.LoadId);
        
        if (load is null)
            return ResponseResult.CreateError($"Could not find load with ID '{req.LoadId}'");

        var updated = false;
        if (req.CanConfirmPickUp.HasValue && req.CanConfirmPickUp != load.CanConfirmPickUp)
        {
            load.CanConfirmPickUp = req.CanConfirmPickUp.Value;
            updated = true;
        }
        if (req.CanConfirmDelivery.HasValue && req.CanConfirmDelivery != load.CanConfirmDelivery)
        {
            load.CanConfirmDelivery = req.CanConfirmDelivery.Value;
            updated = true;
        }
        
        _tenantRepository.Update(load);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();
        
        if (updated && changes > 0)
        {
            await _pushNotification.SendUpdatedLoadNotificationAsync(load);
        }
        return ResponseResult.CreateSuccess();
    }
}
