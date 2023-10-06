namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateNotificationHandler : RequestHandler<UpdateNotificationCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateNotificationHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateNotificationCommand req, CancellationToken cancellationToken)
    {
        var notification = await _tenantRepository.GetAsync<Notification>(req.Id);

        if (notification is null)
        {
            return ResponseResult.CreateError($"Could not find a notification with ID '{req.Id}'");
        }
        
        if (req.IsRead.HasValue && notification.IsRead != req.IsRead)
        {
            notification.IsRead = req.IsRead.Value;
        }
        
        _tenantRepository.Update(notification);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
