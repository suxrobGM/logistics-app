using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

/// <summary>
///     SignalR implementation of real-time notification service.
/// </summary>
internal sealed class SignalRNotificationService(IHubContext<NotificationHub, INotificationHubClient> hubContext)
    : IRealtimeNotificationService
{
    public async Task BroadcastNotificationAsync(
        string tenantId,
        NotificationDto notification,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group(tenantId)
            .ReceiveNotification(notification);
    }
}
