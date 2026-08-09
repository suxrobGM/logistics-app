using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>How tool calls in an agent run are processed.</summary>
/// <param name="CallerPermissions">
/// Null (dispatch runs) disables per-tool permission checks - the endpoint's policy is the gate.
/// Non-null (copilot turns) fails any call whose RequiredPermission the caller lacks.
/// </param>
/// <param name="DecisionBroadcastOverride">
/// Replaces the dispatch-board broadcast; copilot turns route decisions to the conversation owner.
/// </param>
internal sealed record ToolCallContext(
    IReadOnlySet<string>? CallerPermissions = null,
    Func<AgentDecisionDto, Task>? DecisionBroadcastOverride = null);
