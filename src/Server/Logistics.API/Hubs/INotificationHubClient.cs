using Logistics.Models;

namespace Logistics.API.Hubs;

public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationDto notification);
}
