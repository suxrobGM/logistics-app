using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>
///     Hub client for receiving notifications.
/// </summary>
public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationDto notification);
}
