using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>Tenant facts for the dispatch system prompt. The feature flags are named init properties, so two <c>bool</c> arguments cannot be swapped by mistake.</summary>
internal sealed record DispatchPromptContext(string CompanyName)
{
    public DistanceUnit DistanceUnit { get; init; } = DistanceUnit.Miles;

    /// <summary>ISO 4217 code of the tenant currency, which all amounts are in.</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>Swaps the fleet-wide framing for one-truck framing.</summary>
    public bool IsSolo { get; init; }

    public bool HasLoadBoardIntegration { get; init; }

    /// <summary><c>TenantFeature.IntermodalContainers</c>; gates a section naming its tools.</summary>
    public bool HasIntermodal { get; init; }

    /// <summary><c>TenantFeature.VehicleTransport</c>; adds the car hauler rules.</summary>
    public bool HasVehicleTransport { get; init; }

    /// <summary>True when the tenant has <c>TenantFeature.AINegotiation</c> and a connected load board; the negotiation tools need both.</summary>
    public bool HasRateNegotiation { get; init; }

    /// <summary>Null omits the section.</summary>
    public LearnedDispatchPolicy? Policy { get; init; }
}
