using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Date format preferences for tenant localization.
/// </summary>
public enum DateFormatType
{
    [Description("MM/DD/YYYY")]
    US,

    [Description("DD/MM/YYYY")]
    European,

    [Description("YYYY-MM-DD")]
    ISO
}
