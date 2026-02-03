using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum TrialPeriod
{
    None,

    [Description("7 Days")]
    SevenDays,

    [Description("14 Days")]
    FourteenDays,

    [Description("30 Days")]
    ThirtyDays
}
