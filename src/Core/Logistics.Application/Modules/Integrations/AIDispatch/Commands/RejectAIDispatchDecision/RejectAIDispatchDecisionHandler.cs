using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RejectAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchDecisionGuard guard,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService) : IAppRequestHandler<RejectAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var loaded = await guard.LoadAsync(request.DecisionId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded.Error!);

        var decision = loaded.Value!;
        decision.Reject(currentUser.GetUserId() ?? Guid.Empty, request.Reason);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var conversation = await notes.LoadConversationAsync(decision, ct);

        await notes.AppendAsync(
            conversation, notes.RejectionNote(decision, request.Reason),
            message => broadcastService.BroadcastMessageAsync(tenantId, message), ct);

        return Result.Ok();
    }
}
