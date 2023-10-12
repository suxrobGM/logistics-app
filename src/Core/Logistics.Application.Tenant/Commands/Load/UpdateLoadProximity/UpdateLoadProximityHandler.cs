using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;
using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadProximityHandler : RequestHandler<UpdateLoadProximityCommand, ResponseResult>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantRepository _tenantRepository;

    public UpdateLoadProximityHandler(
        ITenantRepository tenantRepository,
        IPushNotificationService pushNotificationService)
    {
        _tenantRepository = tenantRepository;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateLoadProximityCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.LoadId);
        
        if (load is null)
            return ResponseResult.CreateError($"Could not find load with ID '{req.LoadId}'");

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
        
        _tenantRepository.Update(load);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();
        
        if (loadStatus.HasValue && changes > 0)
        {
            await _pushNotificationService.SendConfirmLoadStatusNotificationAsync(load, loadStatus.Value);
        }
        return ResponseResult.CreateSuccess();
    }
}
