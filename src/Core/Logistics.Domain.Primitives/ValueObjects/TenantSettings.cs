using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Geo;

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

    /// <summary>
    /// ISO 639-1 language code used as the tenant's UI/document default. Falls back to "en".
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>Whether AI features may call the LLM; null or true means enabled. False blocks calls on demo or test tenants, except in development.</summary>
    public bool? AIEnabled { get; set; }

    /// <summary>Minimum broker credit score to negotiate or book with a broker; null disables the check. A broker with no score is never blocked; one with inactive FMCSA authority always is.</summary>
    public int? MinBrokerCreditScore { get; set; }

    /// <summary>
    /// Owner-set: pause AI at the weekly budget instead of billing metered overage (default).
    /// </summary>
    public bool BlockAIOverage { get; set; }

    /// <summary>Rate floor per km in tenant currency, used when no lane floor matches a listing; null means the agent negotiates without a floor.</summary>
    public decimal? DefaultRateFloorPerKm { get; set; }

    /// <summary>Fleet operating cost per km in tenant currency, excluding driver pay; null estimates it from each truck's trailing 90-day expenses.</summary>
    public decimal? CostPerKm { get; set; }

    /// <summary>Free time per stop before detention accrues, in minutes; null means the 2-hour industry default. A customer's own terms override it.</summary>
    public int? DetentionFreeMinutes { get; set; }

    /// <summary>Detention charge per hour in tenant currency; null disables detention billing, though the dwell alert still fires.</summary>
    public decimal? DetentionRatePerHour { get; set; }

    /// <summary>Loading or unloading time the ETA allows per stop. Null means 60.</summary>
    public int? StopDwellMinutes { get; set; }

    /// <summary>Each alert costs an agent turn. Null means 20.</summary>
    public int? TripAlertsPerDay { get; set; }
}
