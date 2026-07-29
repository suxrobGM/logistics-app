using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RejectAICopilotDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAICopilotBroadcastService broadcastService) : IAppRequestHandler<RejectAICopilotDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAICopilotDecisionCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var guard = await AICopilotDecisionGuard.LoadAsync(tenantUow, request.DecisionId, userId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var (decision, conversation) = guard.Value!;

        decision.Reject(userId!.Value, request.Reason);

        var note = string.IsNullOrWhiteSpace(request.Reason)
            ? $"Rejected: {decision.ToolName}"
            : $"Rejected: {decision.ToolName} - {request.Reason}";
        var message = conversation.AddTextMessage(AICopilotMessageRole.System, note);
        await tenantUow.SaveChangesAsync(ct);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        await broadcastService.BroadcastMessageAsync(tenantId, conversation.CreatedById, message.ToDto());
        await broadcastService.BroadcastDecisionAsync(tenantId, conversation.CreatedById, decision.ToDto());
        return Result.Ok();
    }
}
