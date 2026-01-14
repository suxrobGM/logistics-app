using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum HosViolationType
{
    [Description("11-Hour Driving Limit")] [EnumMember(Value = "driving_11_hour")]
    Driving11Hour = 1,

    [Description("14-Hour On-Duty Limit")] [EnumMember(Value = "on_duty_14_hour")]
    OnDuty14Hour = 2,

    [Description("30-Minute Break Required")] [EnumMember(Value = "break_30_minute")]
    Break30Minute = 3,

    [Description("70-Hour/8-Day Cycle Limit")] [EnumMember(Value = "cycle_70_hour")]
    Cycle70Hour = 4,

    [Description("34-Hour Restart Required")] [EnumMember(Value = "restart_required")]
    RestartRequired = 5,

    [Description("Form and Manner Violation")] [EnumMember(Value = "form_and_manner")]
    FormAndMannerViolation = 6
}
