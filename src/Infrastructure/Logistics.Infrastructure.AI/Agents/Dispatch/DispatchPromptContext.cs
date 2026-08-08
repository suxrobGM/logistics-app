using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// Everything <see cref="AIDispatchSystemPrompt.Build"/> varies on.
/// </summary>
/// <remarks>
/// Only the company name is positional. Everything else is an init property so each flag is named
/// at the call site - the builder previously took several trailing optionals, where swapping two
/// <c>bool</c>s or two enums compiled cleanly and silently changed the prompt.
/// </remarks>
internal sealed record DispatchPromptContext(string CompanyName)
{
    public DistanceUnit DistanceUnit { get; init; } = DistanceUnit.Miles;

    /// <summary><c>SoloOperator</c> swaps the fleet-wide framing for one-truck framing.</summary>
    public OperatingMode OperatingMode { get; init; } = OperatingMode.Fleet;

    public bool HasLoadBoardIntegration { get; init; }

    /// <summary>
    /// Whether the tenant has <c>TenantFeature.IntermodalContainers</c>. The section costs ~310 tokens
    /// per request and names tools a gated tenant is not given, so it must move in lockstep with them.
    /// </summary>
    public bool HasIntermodal { get; init; }

    /// <summary>The tenant's learned dispatch policy, or null to omit the section.</summary>
    public LearnedDispatchPolicy? Policy { get; init; }

    public bool IsSolo => OperatingMode == OperatingMode.SoloOperator;
}
