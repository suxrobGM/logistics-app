using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

internal sealed class AIDispatchDecisionGuard(
    ITenantUnitOfWork tenantUow,
    IOptions<LlmOptions> llmOptions) : IAIDispatchDecisionGuard
{
    public async Task<Result<AgentDecision>> LoadAsync(Guid decisionId, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!llmOptions.Value.BypassAIGate && tenant.Settings.AIEnabled == false)
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
