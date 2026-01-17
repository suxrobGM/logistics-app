using Logistics.Application.Hubs;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Services;

internal class NotificationService(
    ITenantUnitOfWork tenantUow,
    IHubContext<NotificationHub, INotificationHubClient> notificationHub)
    : INotificationService
{
    public async Task SendNotificationAsync(string title, string message)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        var notification = new Notification
        {
            Title = title,
            Message = message
        };

        var notificationRepository = tenantUow.Repository<Notification>();
        await notificationRepository.AddAsync(notification);
        var changes = await tenantUow.SaveChangesAsync();

        if (changes > 0)
        {
            await notificationHub.Clients.Group(tenantId.ToString())
                .ReceiveNotification(notification.ToDto());
        }
    }
}
