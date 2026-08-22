using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchDecisionGuard guard,
    IAgentDecisionAuthorization authorization,
    IAgentDecisionExecution execution,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAgentRunContext runContext,
    IAIDispatchBroadcastService broadcastService) : IAppRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var loaded = await guard.LoadAsync(request.DecisionId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded.Error!);

        var decision = loaded.Value!;
        var userId = currentUser.GetUserId() ?? Guid.Empty;
        var tenantId = tenantUow.GetCurrentTenant().Id;

        var allowed = await authorization.EnsureToolPermissionAsync(decision, userId, tenantId, ct);
        if (!allowed.IsSuccess)
            return allowed;

        decision.Approve(userId);

        var conversation = await notes.LoadConversationAsync(decision, ct);

        // Approval runs in an HTTP scope with no agent run behind it, so a tool that needs to know
        // which conversation and decision it belongs to has no other source.
        runContext.SetConversation(conversation.Id);
        runContext.SetDecision(decision.Id);

        return await execution.ExecuteAndNoteAsync(
            decision,
            note => notes.AppendAsync(
                conversation, note, userId,
                message => broadcastService.BroadcastMessageAsync(tenantId, message), ct),
            ct);
    }
}
