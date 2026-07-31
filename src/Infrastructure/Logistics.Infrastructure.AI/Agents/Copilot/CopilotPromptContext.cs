using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Everything <see cref="AICopilotSystemPrompt.Build"/> varies on.
/// </summary>
/// <remarks>
/// Only the company name is positional; the rest are named init properties. Deliberately separate
/// from <see cref="Dispatch.DispatchPromptContext"/> even where the fields coincide - the two
/// prompts are independent by design and must stay free to diverge.
/// </remarks>
internal sealed record CopilotPromptContext(string CompanyName)
{
    public DistanceUnit DistanceUnit { get; init; } = DistanceUnit.Miles;

    /// <summary><c>SoloOperator</c> drops the fleet-wide framing.</summary>
    public OperatingMode OperatingMode { get; init; } = OperatingMode.Fleet;

    /// <summary>
    /// Whether the caller's tool set includes the dispatch write tools. The guardrails section
    /// travels with them, so callers without them never pay those tokens.
    /// </summary>
    public bool HasDispatchTools { get; init; }
}
