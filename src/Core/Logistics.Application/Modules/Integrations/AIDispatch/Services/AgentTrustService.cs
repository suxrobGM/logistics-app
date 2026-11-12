using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

internal sealed class AgentTrustService(ITenantUnitOfWork tenantUow) : IAgentTrustService
{
    public async Task<bool> CanAutoExecuteAsync(AgentToolDefinition definition, CancellationToken ct = default)
    {
        if (definition.Destructive || !definition.IsWrite)
        {
            return false;
        }

        var policy = await tenantUow.Repository<AIDispatchPolicy>().Query().FirstOrDefaultAsync(ct);
        if (policy is null || !policy.IsAutoApproveOptedIn(definition.DecisionType))
        {
            return false;
        }

        return await GetStreakAsync(definition.DecisionType, ct) >= policy.AutoApproveStreak;
    }

    public async Task<int> GetStreakAsync(AgentDecisionType type, CancellationToken ct = default)
    {
        var decisions = tenantUow.Repository<AgentDecision>().Query()
            .DispatchOnly()
            .Where(d => d.Type == type);

        var lastBreakAt = await decisions
            .Where(d => d.Status == AgentDecisionStatus.Rejected || d.RevertedAt != null)
            .MaxAsync(d => (DateTime?)d.CreatedAt, ct);

        return await decisions
            .Where(d => d.Status == AgentDecisionStatus.Approved || d.Status == AgentDecisionStatus.Executed)
            .Where(d => d.RevertedAt == null)
            .Where(d => lastBreakAt == null || d.CreatedAt > lastBreakAt)
            .CountAsync(ct);
    }
}
