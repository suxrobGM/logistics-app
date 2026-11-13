using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>Tenant facts for the copilot system prompt. A separate type from DispatchPromptContext so the two prompts can change independently.</summary>
internal sealed record CopilotPromptContext(string CompanyName)
{
    public DistanceUnit DistanceUnit { get; init; } = DistanceUnit.Miles;

    /// <summary>ISO 4217 code of the tenant currency, which all amounts are in.</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>Drops the fleet-wide framing.</summary>
    public bool IsSolo { get; init; }

    /// <summary>Whether dispatch tools are available. Only then does the prompt include the dispatch guardrails section, which would otherwise waste tokens.</summary>
    public bool HasDispatchTools { get; init; }
}
