using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Base for every hub in the platform. Aborts a connection whose JWT carries no usable tenant
///     and user claim, then puts the rest into the group named by <see cref="GroupNameFor"/>.
///     A hub cannot skip the check by forgetting to call base: <c>OnConnectedAsync</c> is sealed,
///     and per-connection bookkeeping goes in <see cref="OnTenantConnectedAsync"/> instead.
/// </summary>
[Authorize]
public abstract class TenantHub<TClient> : Hub<TClient>
    where TClient : class
{
    public sealed override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } userId)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupNameFor(tenantId, userId));
        await OnTenantConnectedAsync(tenantId, userId);
        await base.OnConnectedAsync();
    }

    /// <summary>The group this connection joins. Defaults to the whole tenant.</summary>
    protected virtual string GroupNameFor(Guid tenantId, Guid userId) => tenantId.ToString();

    /// <summary>Runs after the connection joined its group, for hubs that track their clients.</summary>
    protected virtual Task OnTenantConnectedAsync(Guid tenantId, Guid userId) => Task.CompletedTask;
}
