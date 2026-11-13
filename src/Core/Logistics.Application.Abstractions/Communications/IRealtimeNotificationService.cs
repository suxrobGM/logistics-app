using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Communications;

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
