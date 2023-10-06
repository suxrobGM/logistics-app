namespace Logistics.Application.Tenant.Services;

public class NotificationService : INotificationService
{
    private readonly ITenantRepository _tenantRepository;

    public NotificationService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task SendNotificationAsync(string title, string message)
    {
        await _tenantRepository.AddAsync(new Notification
        {
            Title = title,
            Message = message
        });

        await _tenantRepository.UnitOfWork.CommitAsync();
    }
}
