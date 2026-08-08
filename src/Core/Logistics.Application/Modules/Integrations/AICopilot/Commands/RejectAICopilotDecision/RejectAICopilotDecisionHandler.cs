using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AICopilot.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RejectAICopilotDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAICopilotDecisionGuard guard,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAICopilotBroadcastService broadcastService) : IAppRequestHandler<RejectAICopilotDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAICopilotDecisionCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var loaded = await guard.LoadAsync(request.DecisionId, userId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded.Error!);

        var (decision, conversation) = loaded.Value!;
        decision.Reject(userId!.Value, request.Reason);

        var tenantId = tenantUow.GetCurrentTenant().Id;

        await notes.AppendAsync(
            conversation, notes.RejectionNote(decision, request.Reason),
            message => broadcastService.BroadcastMessageAsync(tenantId, conversation.CreatedById, message), ct);

        await broadcastService.BroadcastDecisionAsync(tenantId, conversation.CreatedById, decision.ToDto());
        return Result.Ok();
    }
}
