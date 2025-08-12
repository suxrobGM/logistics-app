using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TrialPeriod
{
    [Description("None"), EnumMember(Value = "none")]
    None,

    [Description("7 Days"), EnumMember(Value = "7d")]
    SevenDays,

    [Description("14 Days"), EnumMember(Value = "14d")]
    FourteenDays,

    [Description("30 Days"), EnumMember(Value = "30d")]
    ThirtyDays
}
