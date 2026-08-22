using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AICopilot.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class ApproveAICopilotDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAICopilotDecisionGuard guard,
    IAgentDecisionAuthorization authorization,
    IAgentDecisionExecution execution,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAgentRunContext runContext,
    IAICopilotBroadcastService broadcastService,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAICopilotDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAICopilotDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!llmOptions.Value.BypassAIGate && tenant.Settings.AIEnabled == false)
            return Result.Fail("AI is disabled for this tenant");

        var userId = currentUser.GetUserId();
        var loaded = await guard.LoadAsync(request.DecisionId, userId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded.Error!);

        var (decision, conversation) = loaded.Value!;

        var allowed = await authorization.EnsureToolPermissionAsync(
            decision, userId!.Value, tenant.Id, ct);
        if (!allowed.IsSuccess)
            return allowed;

        decision.Approve(userId!.Value);

        // Approval runs in an HTTP scope with no agent run behind it, so a tool that needs to know
        // which conversation and decision it belongs to has no other source.
        runContext.SetConversation(conversation.Id);
        runContext.SetDecision(decision.Id);

        var outcome = await execution.ExecuteAndNoteAsync(
            decision,
            note => notes.AppendAsync(
                conversation, note,
                message => broadcastService.BroadcastMessageAsync(tenant.Id, conversation.CreatedById, message), ct),
            ct);

        await broadcastService.BroadcastDecisionAsync(tenant.Id, conversation.CreatedById, decision.ToDto());
        return outcome;
    }
}
