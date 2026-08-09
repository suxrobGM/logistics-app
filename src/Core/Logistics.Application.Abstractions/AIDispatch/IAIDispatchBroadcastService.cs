using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Pushes dispatch conversation events to every connected client for a tenant. Unlike the copilot's
/// per-user groups, dispatch conversations are tenant-shared, so every broadcast targets the whole
/// tenant.
/// </summary>
public interface IAIDispatchBroadcastService
{
    Task BroadcastMessageAsync(Guid tenantId, AgentMessageDto message);
    Task BroadcastTurnUpdateAsync(Guid tenantId, AgentTurnUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, AgentDecisionDto decision);
}
