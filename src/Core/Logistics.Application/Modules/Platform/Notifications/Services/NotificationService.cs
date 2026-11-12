using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

internal sealed class NotificationService(
    ITenantUnitOfWork tenantUow,
    IRealtimeNotificationService realtimeNotificationService)
    : INotificationService
{
    public async Task SendNotificationAsync(string title, string message)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        var notification = new Notification { Title = title, Message = message };

        var notificationRepository = tenantUow.Repository<Notification>();
        await notificationRepository.AddAsync(notification);
        var changes = await tenantUow.SaveChangesAsync();

        if (changes > 0)
        {
            await realtimeNotificationService.BroadcastNotificationAsync(tenantId.ToString(), notification.ToDto());
        }
    }
}
