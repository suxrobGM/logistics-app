using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// Decides when the dispatch agent may execute a write decision without asking, from the tenant's
/// opt-in and its approval history.
/// </summary>
public interface IAgentTrustService : IApplicationService
{
    /// <summary>
    /// True when the tool's decision type is eligible, opted in, not destructive, and has a clean
    /// streak at least as long as the tenant requires.
    /// </summary>
    Task<bool> CanAutoExecuteAsync(AgentToolDefinition definition, CancellationToken ct = default);

    /// <summary>
    /// Approved or executed dispatch decisions of this type since its last rejection or revert.
    /// </summary>
    Task<int> GetStreakAsync(AgentDecisionType type, CancellationToken ct = default);
}
