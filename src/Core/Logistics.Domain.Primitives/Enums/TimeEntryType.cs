using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Type of time entry for hourly employees.
/// </summary>
public enum TimeEntryType
{
    [Description("Regular")] [EnumMember(Value = "regular")]
    Regular,

    [Description("Overtime")] [EnumMember(Value = "overtime")]
    Overtime,

    [Description("Double Time")] [EnumMember(Value = "double_time")]
    DoubleTime,

    [Description("Paid Time Off")] [EnumMember(Value = "pto")]
    PaidTimeOff,

    [Description("Holiday")] [EnumMember(Value = "holiday")]
    Holiday
}
