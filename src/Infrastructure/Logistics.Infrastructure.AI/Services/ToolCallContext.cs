using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// How tool calls in an agent run are processed.
/// </summary>
/// <param name="Mode">HumanInTheLoop turns write tools into suggestions; Autonomous executes them.</param>
/// <param name="CallerPermissions">
/// The triggering user's permission set. Null (dispatch runs) disables per-tool permission checks -
/// the endpoint's policy is the gate. Non-null (copilot turns) fails any tool call whose
/// RequiredPermission the caller lacks.
/// </param>
/// <param name="DecisionBroadcastOverride">
/// Replaces the default dispatch-board broadcast; copilot turns route decisions to the
/// conversation owner's group instead.
/// </param>
internal sealed record ToolCallContext(
    AIDispatchMode Mode,
    IReadOnlySet<string>? CallerPermissions = null,
    Func<AIDispatchDecisionDto, Task>? DecisionBroadcastOverride = null);
