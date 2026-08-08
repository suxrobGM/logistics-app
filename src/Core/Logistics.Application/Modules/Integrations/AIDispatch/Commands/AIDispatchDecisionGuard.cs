using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// Shared by dispatch decision approval and rejection: AI must be enabled for the tenant, and the
/// decision must exist, belong to a dispatch turn, and still be Suggested.
/// </summary>
internal static class AIDispatchDecisionGuard
{
    public static async Task<Result<AgentDecision>> LoadAsync(
        ITenantUnitOfWork tenantUow, LlmOptions llmOptions, Guid decisionId, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!llmOptions.BypassAIGate && tenant.Settings.AIEnabled == false)
            return Result<AgentDecision>.Fail("AI dispatch is disabled for this tenant");

        var decision = await tenantUow.Repository<AgentDecision>()
            .Query()
            .DispatchOnly()
            .FirstOrDefaultAsync(d => d.Id == decisionId, ct);

        if (decision is null)
            return Result<AgentDecision>.Fail("Decision not found");

        if (decision.Status != AgentDecisionStatus.Suggested)
            return Result<AgentDecision>.Fail("Decision is not in a suggested state");

        return Result<AgentDecision>.Ok(decision);
    }
}
