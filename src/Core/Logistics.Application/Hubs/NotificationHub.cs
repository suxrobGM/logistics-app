using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Application.Hubs;

public class NotificationHub : Hub<INotificationHubClient>
{
    public async Task SendNotification(string tenantId, NotificationDto notification) =>
        await Clients.Group(tenantId).ReceiveNotification(notification);

    public async Task RegisterTenant(string tenantId) => await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);

    public async Task UnregisterTenant(string tenantId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
}
