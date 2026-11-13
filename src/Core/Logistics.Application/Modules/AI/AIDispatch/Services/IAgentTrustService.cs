using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Services;

public interface IAgentTrustService : IApplicationService
{
    /// <summary>Requires an eligible, opted-in, non-destructive type with a long enough clean streak.</summary>
    Task<bool> CanAutoExecuteAsync(AgentToolDefinition definition, CancellationToken ct = default);

    /// <summary>Approved or executed decisions of this type since its last rejection or revert.</summary>
    Task<int> GetStreakAsync(AgentDecisionType type, CancellationToken ct = default);
}
