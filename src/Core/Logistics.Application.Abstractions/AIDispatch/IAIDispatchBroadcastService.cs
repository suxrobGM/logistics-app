using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Service for broadcasting AI dispatch agent updates to connected clients.
/// </summary>
public interface IAIDispatchBroadcastService
{
    Task BroadcastSessionUpdateAsync(Guid tenantId, AgentSessionUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, AgentDecisionDto decision);
}
