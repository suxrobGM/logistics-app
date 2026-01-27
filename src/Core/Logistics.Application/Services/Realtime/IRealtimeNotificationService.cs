using Logistics.Shared.Models;

namespace Logistics.Application.Services.Realtime;

/// <summary>
///     Abstraction for real-time notifications.
/// </summary>
public interface IRealtimeNotificationService
{
    Task BroadcastNotificationAsync(
        string tenantId,
        NotificationDto notification,
        CancellationToken ct = default);
}
