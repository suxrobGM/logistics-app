using Logistics.Models;

namespace Logistics.Application.Tenant.Commands;

internal sealed class ConfirmLoadStatusHandler : RequestHandler<ConfirmLoadStatusCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public ConfirmLoadStatusHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        ConfirmLoadStatusCommand req, CancellationToken cancellationToken)
    {
        var tenantId = _tenantRepository.GetCurrentTenant().Id;
        var load = await _tenantRepository.GetAsync<Load>(req.LoadId);
        
        if (load is null)
            return ResponseResult.CreateError($"Could not find load with ID '{req.LoadId}'");

        var loadStatus = req.LoadStatus!.Value;
        load.SetStatus(loadStatus);
        
        _tenantRepository.Update(load);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();

        if (changes > 0)
        {
            await SendNotificationAsync(req, tenantId, load);
        }
        
        return ResponseResult.CreateSuccess();
    }

    private async Task SendNotificationAsync(ConfirmLoadStatusCommand req, string tenantId, Load load)
    {
        if (req.SendNotificationAsync is null)
        {
            return;
        }
        
        var notification = new NotificationDto
        {
            Title = "Load updates",
            Message = $"Driver confirmed the load #{load.RefId} status to '{load.GetStatus()}'"
        };

        await req.SendNotificationAsync(tenantId, notification);
    }
}
