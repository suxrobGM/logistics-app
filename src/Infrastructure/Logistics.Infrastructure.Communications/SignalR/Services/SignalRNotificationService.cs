using Logistics.Application.Services.Realtime;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRNotificationService : IRealtimeNotificationService
{
    public Task BroadcastNotificationAsync(string tenantId, NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
