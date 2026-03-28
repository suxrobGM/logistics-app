using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Regional and localization settings for a tenant.
/// </summary>
[ComplexType]
public record TenantSettings
{
    public DistanceUnit DistanceUnit { get; set; } = DistanceUnit.Miles;
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;
    public DateFormatType DateFormat { get; set; } = DateFormatType.US;
    public string Timezone { get; set; } = "America/New_York";
    public WeightUnit WeightUnit { get; set; } = WeightUnit.Pounds;

    /// <summary>
    /// Whether LLM API calls are enabled for this tenant. Null/true = enabled, false = blocked.
    /// Used to prevent LLM usage on demo/test tenants in production.
    /// Bypassed in development environments.
    /// </summary>
    public bool? LlmEnabled { get; set; }

    /// <summary>
    /// Selected LLM model for dispatch agent. Validated against the plan's AllowedModelTier.
    /// Null uses the system default.
    /// </summary>
    public string? LlmModel { get; set; }

    /// <summary>
    /// Override extended thinking for dispatch agent. Null uses the system default.
    /// </summary>
    public bool? LlmExtendedThinking { get; set; }

    /// <summary>
    /// Selected LLM provider. Null uses the system default.
    /// </summary>
    public LlmProvider? LlmProvider { get; set; }
}
