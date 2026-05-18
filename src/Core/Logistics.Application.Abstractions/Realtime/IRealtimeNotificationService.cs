using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Abstractions.Realtime;

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
