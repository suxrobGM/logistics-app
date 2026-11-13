using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AICopilot.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class ApproveAICopilotDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAICopilotDecisionGuard guard,
    IAgentDecisionApprover approver,
    ICurrentUserService currentUser,
    IAICopilotBroadcastService broadcastService,
    IAgentDecisionDtoMapper decisionDtoMapper) : IRequestHandler<ApproveAICopilotDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAICopilotDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var userId = currentUser.GetUserId();
        var loaded = await guard.LoadAsync(request.DecisionId, userId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded);

        var (decision, conversation) = loaded.Value!;

        var outcome = await approver.ApproveAsync(
            decision, conversation, userId!.Value, tenant.Id,
            message => broadcastService.BroadcastMessageAsync(tenant.Id, conversation.CreatedById, message), ct);

        await broadcastService.BroadcastDecisionAsync(
            tenant.Id, conversation.CreatedById, await decisionDtoMapper.MapAsync(decision, ct));
        return outcome;
    }
}
