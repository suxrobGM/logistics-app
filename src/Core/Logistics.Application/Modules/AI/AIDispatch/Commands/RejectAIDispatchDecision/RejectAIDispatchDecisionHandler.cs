using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.AIDispatch.Services;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class RejectAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchDecisionGuard guard,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService) : IRequestHandler<RejectAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var loaded = await guard.LoadAsync(request.DecisionId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded);

        var decision = loaded.Value!;
        var userId = currentUser.GetUserId() ?? Guid.Empty;
        decision.Reject(userId, request.Reason);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var conversation = await notes.LoadConversationAsync(decision, ct);

        await notes.AppendAsync(
            conversation, AgentDecisionNote.Rejected(decision, request.Reason), userId,
            message => broadcastService.BroadcastMessageAsync(tenantId, message), ct);

        return Result.Ok();
    }
}
