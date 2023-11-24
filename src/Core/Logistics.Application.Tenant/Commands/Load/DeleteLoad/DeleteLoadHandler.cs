using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteLoadHandler : RequestHandler<DeleteLoadCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public DeleteLoadHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteLoadCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);
        var truck = load?.AssignedTruck;
        
        _tenantUow.Repository<Load>().Delete(load);
        var changes = await _tenantUow.SaveChangesAsync();
        
        if (load is not null && changes > 0)
        {
            await _pushNotificationService.SendRemovedLoadNotificationAsync(load, truck);
        }
        
        return ResponseResult.CreateSuccess();
    }
}
