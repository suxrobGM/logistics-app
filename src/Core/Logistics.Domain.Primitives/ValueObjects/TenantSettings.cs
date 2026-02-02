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
}
