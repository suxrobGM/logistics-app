using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Tenant-wide notifications. Group membership comes from the caller's JWT tenant claim,
///     never from a client-supplied id.
/// </summary>
[Authorize]
public class NotificationHub : Hub<INotificationHubClient>
{
    public override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId.ToString());
        await base.OnConnectedAsync();
    }

    [Obsolete("Identity comes from JWT claims; remove once the driver app stops calling it.")]
    public Task RegisterTenant(string tenantId) => Task.CompletedTask;

    [Obsolete("Identity comes from JWT claims; remove once the driver app stops calling it.")]
    public Task UnregisterTenant(string tenantId) => Task.CompletedTask;
}
