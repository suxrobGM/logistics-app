using Logistics.Shared.Models;

namespace Logistics.Application.Services;

/// <summary>
/// Service for broadcasting AI dispatch agent updates to connected clients.
/// </summary>
public interface IDispatchAgentBroadcastService
{
    Task BroadcastSessionUpdateAsync(Guid tenantId, DispatchAgentUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, DispatchDecisionDto decision);
}
