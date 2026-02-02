using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Supported currency codes for tenant localization.
/// </summary>
public enum CurrencyCode
{
    [Description("US Dollar"), EnumMember(Value = "usd")]
    USD

    // Future currencies:
    // [Description("Euro")] EUR,
    // [Description("British Pound")] GBP,
    // [Description("Canadian Dollar")] CAD,
    // [Description("Mexican Peso")] MXN,
    // [Description("Australian Dollar")] AUD
}
