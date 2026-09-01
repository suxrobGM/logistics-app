using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Copilot conversations are private, so the group is per-user rather than per-tenant. Both
///     ids come from the caller's JWT claims.
/// </summary>
[Authorize]
public class CopilotHub : Hub<IAICopilotHubClient>
{
    public override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } userId)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(tenantId, userId));
        await base.OnConnectedAsync();
    }

    /// <summary>The single source of the group-name shape, shared with the broadcaster.</summary>
    public static string GroupName(Guid tenantId, Guid userId) => $"copilot:{tenantId}:{userId}";
}
