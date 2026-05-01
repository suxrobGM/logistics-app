using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Supported currency codes for tenant localization.
/// </summary>
public enum CurrencyCode
{
    [Description("US Dollar")]
    USD,

    [Description("Euro")]
    EUR

    // Future currencies:
    // [Description("British Pound")] GBP,
    // [Description("Canadian Dollar")] CAD,
    // [Description("Mexican Peso")] MXN,
    // [Description("Australian Dollar")] AUD
}
