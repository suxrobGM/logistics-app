using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadProximityHandler : RequestHandler<UpdateLoadProximityCommand, Result>
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

    protected override async Task<Result> HandleValidated(
        UpdateLoadProximityCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
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
        return Result.Succeed();
    }
}
