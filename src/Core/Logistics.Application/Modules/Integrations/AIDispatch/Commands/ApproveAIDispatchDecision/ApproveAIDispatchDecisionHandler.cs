using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAgentToolExecutor toolExecutor,
    IAgentToolRegistry toolRegistry,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService,
    IMediator mediator,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var guard = await AIDispatchDecisionGuard.LoadAsync(
            tenantUow, llmOptions.Value, request.DecisionId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var decision = guard.Value!;
        var userId = currentUser.GetUserId() ?? Guid.Empty;
        var tenantId = tenantUow.GetCurrentTenant().Id;

        // Dispatch.Manage alone is not enough to execute, say, an invoice write.
        if (toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission is { } requiredPermission)
        {
            var permissions = await mediator.Send(new GetCurrentUserPermissionsQuery
            {
                UserId = userId,
                TenantId = tenantId
            }, ct);

            if (permissions.Value?.Contains(requiredPermission) != true)
                return Result.Fail($"You need the {requiredPermission} permission to approve this action");
        }

        decision.Approve(userId);

        var conversation = await AgentDecisionNotes.LoadConversationAsync(tenantUow, decision, ct);

        return await AgentDecisionExecution.ExecuteAndNoteAsync(
            toolExecutor,
            decision,
            note => AgentDecisionNotes.AppendAsync(
                tenantUow, conversation, note,
                message => broadcastService.BroadcastMessageAsync(tenantId, message), ct),
            ct);
    }
}
