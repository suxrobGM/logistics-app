using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Agents.Services;

/// <summary>
/// What a dispatcher did with a decision. <see cref="ForAgent"/> is replayed to the LLM and keeps the
/// tool name and raw output; <see cref="ForUser"/> is the transcript line people read.
/// </summary>
internal sealed record AgentDecisionNote(string ForAgent, string ForUser)
{
    public static AgentDecisionNote Executed(AgentDecision decision, string output) => new(
        $"Approved and executed: {decision.ToolName} - {ToolErrorSanitizer.Truncate(output)}",
        $"Approved: {LabelOf(decision)}");

    public static AgentDecisionNote Failed(AgentDecision decision, string error) => new(
        $"Approved but failed to execute: {decision.ToolName} - {ToolErrorSanitizer.Truncate(error)}",
        $"Approved, but {LabelOf(decision)} failed: {ToolErrorSanitizer.Truncate(error)}");

    public static AgentDecisionNote Rejected(AgentDecision decision, string? reason)
    {
        var suffix = string.IsNullOrWhiteSpace(reason) ? "" : $" - {reason}";
        return new($"Rejected: {decision.ToolName}{suffix}", $"Rejected: {LabelOf(decision)}{suffix}");
    }

    public static AgentDecisionNote Reverted(AgentDecision decision) => new(
        $"Reverted by a dispatcher: {decision.ToolName}",
        $"Reverted: {LabelOf(decision)}");

    /// <summary>"assign_load_to_truck" reads as "Assign load to truck".</summary>
    private static string LabelOf(AgentDecision decision)
    {
        var words = (decision.ToolName ?? "action").Replace('_', ' ');
        return char.ToUpperInvariant(words[0]) + words[1..];
    }
}
