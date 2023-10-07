using Logistics.Shared.Models;

namespace Logistics.API.Hubs;

public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationDto notification);
}
