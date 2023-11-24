using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;
using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadProximityHandler : RequestHandler<UpdateLoadProximityCommand, ResponseResult>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateLoadProximityHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateLoadProximityCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return ResponseResult.CreateError($"Could not find load with ID '{req.LoadId}'");
        }
        
        LoadStatus? loadStatus = null;
        if (req.CanConfirmPickUp.HasValue && req.CanConfirmPickUp != load.CanConfirmPickUp)
        {
            load.CanConfirmPickUp = req.CanConfirmPickUp.Value;
            loadStatus = LoadStatus.PickedUp;
        }
        if (req.CanConfirmDelivery.HasValue && req.CanConfirmDelivery != load.CanConfirmDelivery)
        {
            load.CanConfirmDelivery = req.CanConfirmDelivery.Value;
            loadStatus = LoadStatus.Delivered;
        }
        
        _tenantUow.Repository<Load>().Update(load);
        var changes = await _tenantUow.SaveChangesAsync();
        
        if (loadStatus.HasValue && changes > 0)
        {
            await _pushNotificationService.SendConfirmLoadStatusNotificationAsync(load, loadStatus.Value);
        }
        return ResponseResult.CreateSuccess();
    }
}
