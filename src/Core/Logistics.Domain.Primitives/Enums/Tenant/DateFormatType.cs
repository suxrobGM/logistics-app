using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Date format preferences for tenant localization.
/// </summary>
public enum DateFormatType
{
    [Description("MM/DD/YYYY"), EnumMember(Value = "us")]
    US,

    [Description("DD/MM/YYYY"), EnumMember(Value = "european")]
    European,

    [Description("YYYY-MM-DD"), EnumMember(Value = "iso")]
    ISO
}
