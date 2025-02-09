using Logistics.Shared.Models;

namespace Logistics.Application.Hubs;

/// <summary>
/// Hub client for receiving notifications.
/// </summary>
public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationDto notification);
}
