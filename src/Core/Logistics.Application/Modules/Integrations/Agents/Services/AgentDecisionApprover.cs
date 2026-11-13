using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentDecisionApprover(
    IAgentToolRegistry toolRegistry,
    IUserPermissionService userPermissions,
    IAgentToolExecutor toolExecutor,
    IAgentDecisionNotes notes,
    IAgentRunContext runContext) : IAgentDecisionApprover
{
    private const int NoteOutputLimit = 500;

    public async Task<Result> ApproveAsync(
        AgentDecision decision,
        AgentConversation conversation,
        Guid userId,
        Guid tenantId,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct)
    {
        var required = toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission;
        var allowed = required is null || await userPermissions.HasPermissionAsync(userId, tenantId, required, ct);
        if (!allowed)
        {
            return Result.Fail($"You need the {required} permission to approve this action");
        }

        decision.Approve(userId);

        // Approval runs in an HTTP scope with no agent run behind it, so a tool that needs to know
        // which conversation and decision it belongs to has no other source.
        runContext.SetConversation(conversation.Id);
        runContext.SetDecision(decision.Id);

        var (outcome, note) = await ExecuteAsync(decision, ct);
        await notes.AppendAsync(conversation, note, userId, broadcastMessageAsync, ct);
        return outcome;
    }

    private async Task<(Result Outcome, string Note)> ExecuteAsync(AgentDecision decision, CancellationToken ct)
    {
        try
        {
            var output = await toolExecutor.ExecuteToolAsync(decision.ToolName!, decision.ToolInput!, ct);
            var parsed = ToolOutput.Parse(output);
            if (ToolOutput.IsFailure(parsed))
            {
                var error = parsed?["error"]?.ToString() ?? "The tool reported a failure";
                decision.MarkFailed(output);
                return (Result.Fail($"Failed to execute decision: {error}"),
                    $"Approved but failed to execute: {decision.ToolName} - {Compact(error)}");
            }

            decision.ToolOutput = output;
            decision.MarkExecuted();
            return (Result.Ok(), $"Approved and executed: {decision.ToolName} - {Compact(output)}");
        }
        catch (Exception ex)
        {
            var error = ToolErrorSanitizer.Sanitize(ex);
            decision.MarkFailed(error);
            return (Result.Fail($"Failed to execute decision: {error}"),
                $"Approved but failed to execute: {decision.ToolName} - {Compact(error)}");
        }
    }

    private static string Compact(string text) =>
        text.Length > NoteOutputLimit ? text[..NoteOutputLimit] : text;
}
