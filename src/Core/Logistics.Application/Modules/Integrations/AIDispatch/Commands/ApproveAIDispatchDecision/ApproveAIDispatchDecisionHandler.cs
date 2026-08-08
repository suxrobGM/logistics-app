using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Domain.Persistence;
using MediatR;

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

        // Dispatch.Manage alone is not enough to execute, say, an invoice write.
        if (toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission is { } requiredPermission)
        {
            var permissions = await mediator.Send(new GetCurrentUserPermissionsQuery
            {
                UserId = userId,
                TenantId = tenantUow.GetCurrentTenant().Id
            }, ct);

            if (permissions.Value?.Contains(requiredPermission) != true)
                return Result.Fail($"You need the {requiredPermission} permission to approve this action");
        }

        decision.Approve(userId);

        string note;
        Result outcome;
        try
        {
            var result = await toolExecutor.ExecuteToolAsync(
                decision.ToolName!, decision.ToolInput!, ct);
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

        await AIDispatchDecisionNotes.AppendAsync(tenantUow, broadcastService, decision, note, ct);
        return outcome;
    }

    private static string Compact(string text) =>
        text.Length > 500 ? text[..500] : text;
}
