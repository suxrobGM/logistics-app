using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// Service for broadcasting AI dispatch agent updates to connected clients.
/// </summary>
public interface IAiDispatchBroadcastService
{
    Task BroadcastSessionUpdateAsync(Guid tenantId, AiDispatchUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, AiDispatchDecisionDto decision);
}
