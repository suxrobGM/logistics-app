using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Regional and localization settings for a tenant.
/// </summary>
[ComplexType]
public record TenantSettings
{
    public Region Region { get; set; } = Region.US;
    public DistanceUnit DistanceUnit { get; set; } = DistanceUnit.Miles;
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;
    public DateFormatType DateFormat { get; set; } = DateFormatType.US;
    public string Timezone { get; set; } = "America/New_York";
    public WeightUnit WeightUnit { get; set; } = WeightUnit.Pounds;
    public VolumeUnit VolumeUnit { get; set; } = VolumeUnit.Gallons;
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Fahrenheit;

    /// <summary>Drives the UX preset (hidden team surfaces, dashboard layout) and the AI dispatch prompt.</summary>
    public OperatingMode OperatingMode { get; set; } = OperatingMode.Fleet;

    /// <summary>
    /// ISO 639-1 language code used as the tenant's UI/document default. Falls back to "en".
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Whether AI (LLM API calls) is enabled for this tenant. Null/true = enabled, false = blocked.
    /// Used to prevent LLM usage on demo/test tenants in production.
    /// Bypassed in development environments.
    /// </summary>
    public bool? AIEnabled { get; set; }

    /// <summary>
    /// Minimum broker credit score (0-100) required to book a load-board load.
    /// Null disables the gate. A missing score never blocks; inactive FMCSA authority always does.
    /// </summary>
    public int? MinBrokerCreditScore { get; set; }

    /// <summary>
    /// Owner-set: pause AI at the weekly budget instead of billing metered overage (default).
    /// </summary>
    public bool BlockAIOverage { get; set; }

    /// <summary>
    /// Fallback minimum rate per mile the rate-negotiation agent may accept when no lane rule matches.
    /// Null leaves the agent without a floor.
    /// </summary>
    public decimal? DefaultRateFloorPerMile { get; set; }
}
