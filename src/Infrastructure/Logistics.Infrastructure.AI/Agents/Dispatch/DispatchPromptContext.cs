using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// Everything <see cref="AIDispatchSystemPrompt.Build"/> varies on. Flags are named init properties
/// so two swapped <c>bool</c>s cannot compile into a different prompt.
/// </summary>
internal sealed record DispatchPromptContext(string CompanyName)
{
    public DistanceUnit DistanceUnit { get; init; } = DistanceUnit.Miles;

    /// <summary>Swaps the fleet-wide framing for one-truck framing.</summary>
    public bool IsSolo { get; init; }

    public bool HasLoadBoardIntegration { get; init; }

    /// <summary><c>TenantFeature.IntermodalContainers</c>; gates a section naming its tools.</summary>
    public bool HasIntermodal { get; init; }

    /// <summary><c>TenantFeature.VehicleTransport</c>; adds the car hauler rules.</summary>
    public bool HasVehicleTransport { get; init; }

    /// <summary><c>TenantFeature.AIRateNegotiation</c> and a load board - the tools need both.</summary>
    public bool HasRateNegotiation { get; init; }

    /// <summary>Null omits the section.</summary>
    public LearnedDispatchPolicy? Policy { get; init; }
}
