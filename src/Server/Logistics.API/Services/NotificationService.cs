using Logistics.API.Hubs;
using Logistics.Application.Tenant.Mappers;
using Logistics.Application.Tenant.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Services;

public class NotificationService : INotificationService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _notificationHub;

    public NotificationService(
        ITenantRepository tenantRepository,
        IHubContext<NotificationHub, INotificationHubClient> notificationHub)
    {
        _tenantRepository = tenantRepository;
        _notificationHub = notificationHub;
    }

    public async Task SendNotificationAsync(string title, string message)
    {
        var tenantId = _tenantRepository.GetCurrentTenant().Id;
        var notification = new Notification
        {
            Title = title,
            Message = message
        };
        
        await _tenantRepository.AddAsync(notification);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();

        if (changes > 0)
        {
            await _notificationHub.Clients.Group(tenantId).ReceiveNotification(notification.ToDto());
        }
    }
}
