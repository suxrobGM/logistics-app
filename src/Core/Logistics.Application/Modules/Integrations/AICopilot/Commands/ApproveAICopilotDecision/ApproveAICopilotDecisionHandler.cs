using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class ApproveAICopilotDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAgentToolExecutor toolExecutor,
    IAgentToolRegistry toolRegistry,
    IAICopilotBroadcastService broadcastService,
    IMediator mediator,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAICopilotDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAICopilotDecisionCommand request, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!llmOptions.Value.BypassAIGate && tenant.Settings.AIEnabled == false)
            return Result.Fail("AI is disabled for this tenant");

        var userId = currentUser.GetUserId();
        var guard = await AICopilotDecisionGuard.LoadAsync(tenantUow, request.DecisionId, userId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var (decision, conversation) = guard.Value!;

        // Copilot.Manage alone is not enough to execute, say, an invoice write.
        if (toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission is { } requiredPermission)
        {
            var permissions = await mediator.Send(new GetCurrentUserPermissionsQuery
            {
                UserId = userId!.Value,
                TenantId = tenant.Id
            }, ct);

            if (permissions.Value?.Contains(requiredPermission) != true)
                return Result.Fail($"You need the {requiredPermission} permission to approve this action");
        }

        decision.Approve(userId!.Value);

        var outcome = await AgentDecisionExecution.ExecuteAndNoteAsync(
            toolExecutor,
            decision,
            note => AgentDecisionNotes.AppendAsync(
                tenantUow, conversation, note,
                message => broadcastService.BroadcastMessageAsync(tenant.Id, conversation.CreatedById, message), ct),
            ct);

        await broadcastService.BroadcastDecisionAsync(tenant.Id, conversation.CreatedById, decision.ToDto());
        return outcome;
    }
}
