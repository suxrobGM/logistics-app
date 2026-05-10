using Logistics.Shared.Models;

namespace Logistics.Application.Services;

/// <summary>
/// Service for broadcasting AI dispatch agent updates to connected clients.
/// </summary>
public interface IAiDispatchBroadcastService
{
    Task BroadcastSessionUpdateAsync(Guid tenantId, AiDispatchUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, AiDispatchDecisionDto decision);
}
