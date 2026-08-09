using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentDecisionExecution(IAgentToolExecutor toolExecutor) : IAgentDecisionExecution
{
    private const int NoteOutputLimit = 500;

    public async Task<Result> ExecuteAndNoteAsync(
        AgentDecision decision, Func<string, Task> appendNoteAsync, CancellationToken ct)
    {
        string note;
        Result outcome;

        try
        {
            var output = await toolExecutor.ExecuteToolAsync(decision.ToolName!, decision.ToolInput!, ct);
            decision.ToolOutput = output;
            decision.MarkExecuted();
            note = $"Approved and executed: {decision.ToolName} - {Compact(output)}";
            outcome = Result.Ok();
        }
        catch (Exception ex)
        {
            decision.MarkFailed(ex.Message);
            note = $"Approved but failed to execute: {decision.ToolName} - {Compact(ex.Message)}";
            outcome = Result.Fail($"Failed to execute decision: {ex.Message}");
        }

        await appendNoteAsync(note);
        return outcome;
    }

    private static string Compact(string text) =>
        text.Length > NoteOutputLimit ? text[..NoteOutputLimit] : text;
}
