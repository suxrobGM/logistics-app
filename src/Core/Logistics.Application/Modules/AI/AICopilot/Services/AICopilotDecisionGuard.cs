using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.AI.AICopilot.Services;

internal sealed class AICopilotDecisionGuard(
    ITenantUnitOfWork tenantUow,
    IAgentConversationAccess access,
    IOptions<LlmOptions> llmOptions) : IAICopilotDecisionGuard
{
    public async Task<Result<CopilotDecisionContext>> LoadAsync(
        Guid decisionId, Guid? userId, CancellationToken ct)
    {
        if (!llmOptions.Value.BypassAIGate && tenantUow.GetCurrentTenant().Settings.AIEnabled == false)
            return Result<CopilotDecisionContext>.Fail("AI is disabled for this tenant");

        if (userId is null)
            return Result<CopilotDecisionContext>.Fail("User is not authenticated");

        var decision = await tenantUow.Repository<AgentDecision>().GetByIdAsync(decisionId, ct);
        if (decision is null || decision.Session.Type != AgentSessionType.Copilot)
            return Result<CopilotDecisionContext>.Fail("Decision not found");

        var conversation = await access.LoadAsync(
            decision.Session.ConversationId, AgentConversationScope.Copilot(userId), ct);
        if (conversation is null)
            return Result<CopilotDecisionContext>.Fail("Decision not found");

        if (decision.Status != AgentDecisionStatus.Suggested)
            return Result<CopilotDecisionContext>.Fail("Decision is not in a suggested state");

        return Result<CopilotDecisionContext>.Ok(new CopilotDecisionContext(decision, conversation));
    }
}
