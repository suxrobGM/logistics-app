using Logistics.Shared.Identity.Claims;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Dispatch-board updates. Authorized, and the board group is derived from the caller's JWT
///     tenant claim - never from a client-supplied id - so a client can only ever watch its own
///     tenant's board. Mirrors <see cref="CopilotHub"/>.
/// </summary>
[Authorize]
public class AIDispatchHub : Hub<IAIDispatchHubClient>
{
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
        if (tenantId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"{DispatchBoardGroupPrefix}{tenantId}");
        await base.OnConnectedAsync();
    }

    // Kept for client compatibility; membership is established from the claim on connect and the
    // client-supplied id is deliberately ignored so a client cannot watch another tenant's board.
    public Task SubscribeToDispatchBoard(string tenantId) => Task.CompletedTask;

    public Task UnsubscribeFromDispatchBoard(string tenantId) => Task.CompletedTask;
}
