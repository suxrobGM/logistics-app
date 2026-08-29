using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Tenant-wide notifications. Authorized, and group membership comes from the caller's JWT
///     tenant claim - never from a client-supplied id - so a client can only ever receive its own
///     tenant's notifications. Mirrors <see cref="CopilotHub"/>.
/// </summary>
[Authorize]
public class NotificationHub : Hub<INotificationHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
        if (tenantId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        await base.OnConnectedAsync();
    }

    // Kept for client compatibility, but tenant membership is established from the claim on connect;
    // the client-supplied id is deliberately ignored so a client cannot join another tenant's group.
    public Task RegisterTenant(string tenantId) => Task.CompletedTask;

    public Task UnregisterTenant(string tenantId) => Task.CompletedTask;
}
