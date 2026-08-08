using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class ApproveAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IAgentToolExecutor toolExecutor,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<ApproveAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(ApproveAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var guard = await AIDispatchDecisionGuard.LoadAsync(
            tenantUow, llmOptions.Value, request.DecisionId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var decision = guard.Value!;
        decision.Approve(currentUser.GetUserId() ?? Guid.Empty);

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
