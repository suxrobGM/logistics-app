using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Agents.Services;

/// <summary>
/// What a dispatcher did with a decision. <see cref="ForAgent"/> is replayed to the LLM and keeps the
/// tool name and raw output; <see cref="ForUser"/> is the transcript line people read.
/// </summary>
internal sealed record AgentDecisionNote(string ForAgent, string ForUser)
{
    private const int OutputLimit = 500;

    public static AgentDecisionNote Executed(AgentDecision decision, string output) => new(
        $"Approved and executed: {decision.ToolName} - {Truncate(output)}",
        $"Approved: {LabelOf(decision)}");

    public static AgentDecisionNote Failed(AgentDecision decision, string error) => new(
        $"Approved but failed to execute: {decision.ToolName} - {Truncate(error)}",
        $"Approved, but {LabelOf(decision)} failed: {Truncate(error)}");

    public static AgentDecisionNote Rejected(AgentDecision decision, string? reason) =>
        string.IsNullOrWhiteSpace(reason)
            ? new($"Rejected: {decision.ToolName}", $"Rejected: {LabelOf(decision)}")
            : new($"Rejected: {decision.ToolName} - {reason}", $"Rejected: {LabelOf(decision)} - {reason}");

    public static AgentDecisionNote Reverted(AgentDecision decision) => new(
        $"Reverted by a dispatcher: {decision.ToolName}",
        $"Reverted: {LabelOf(decision)}");

    /// <summary>"assign_load_to_truck" reads as "Assign load to truck".</summary>
    private static string LabelOf(AgentDecision decision)
    {
        var words = (decision.ToolName ?? "action").Replace('_', ' ');
        return char.ToUpperInvariant(words[0]) + words[1..];
    }

    private static string Truncate(string text) => text.Length > OutputLimit ? text[..OutputLimit] : text;
}
