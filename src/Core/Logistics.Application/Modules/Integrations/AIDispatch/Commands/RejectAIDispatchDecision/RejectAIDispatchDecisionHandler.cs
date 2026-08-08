using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RejectAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<RejectAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var guard = await AIDispatchDecisionGuard.LoadAsync(
            tenantUow, llmOptions.Value, request.DecisionId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var decision = guard.Value!;
        decision.Reject(currentUser.GetUserId() ?? Guid.Empty, request.Reason);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var conversation = await AgentDecisionNotes.LoadConversationAsync(tenantUow, decision, ct);

        await AgentDecisionNotes.AppendAsync(
            tenantUow, conversation, AgentDecisionNotes.RejectionNote(decision, request.Reason),
            message => broadcastService.BroadcastMessageAsync(tenantId, message), ct);

        return Result.Ok();
    }
}
