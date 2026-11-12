using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchDecisionGuard guard,
    IAgentDecisionApprover approver,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService) : IRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var loaded = await guard.LoadAsync(request.DecisionId, ct);
        if (!loaded.IsSuccess)
            return Result.Fail(loaded.Error!);

        var decision = loaded.Value!;
        var userId = currentUser.GetUserId() ?? Guid.Empty;
        var tenantId = tenantUow.GetCurrentTenant().Id;

        return await approver.ApproveAsync(
            decision, userId, tenantId, message => broadcastService.BroadcastMessageAsync(tenantId, message), ct);
    }
}
