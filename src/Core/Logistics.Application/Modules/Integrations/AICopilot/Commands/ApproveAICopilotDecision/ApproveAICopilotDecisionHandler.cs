using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
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
        if (!llmOptions.Value.BypassLlmGate && tenant.Settings.LlmEnabled == false)
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

        string note;
        Result outcome;
        try
        {
            var result = await toolExecutor.ExecuteToolAsync(decision.ToolName!, decision.ToolInput!, ct);
            decision.ToolOutput = result;
            decision.MarkExecuted();
            note = $"Approved and executed: {decision.ToolName} - {Compact(result)}";
            outcome = Result.Ok();
        }
        catch (Exception ex)
        {
            decision.MarkFailed(ex.Message);
            note = $"Approved but failed to execute: {decision.ToolName} - {Compact(ex.Message)}";
            outcome = Result.Fail($"Failed to execute decision: {ex.Message}");
        }

        var message = conversation.AddTextMessage(AICopilotMessageRole.System, note);
        await tenantUow.SaveChangesAsync(ct);

        await broadcastService.BroadcastMessageAsync(tenant.Id, conversation.CreatedById, message.ToDto());
        await broadcastService.BroadcastDecisionAsync(tenant.Id, conversation.CreatedById, decision.ToDto());
        return outcome;
    }

    private static string Compact(string text) =>
        text.Length > 500 ? text[..500] : text;
}
