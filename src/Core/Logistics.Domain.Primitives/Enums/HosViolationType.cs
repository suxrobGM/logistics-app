using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum HosViolationType
{
    [Description("11-Hour Driving Limit")]
    Driving11Hour = 1,

    [Description("14-Hour On-Duty Limit")]
    OnDuty14Hour = 2,

    [Description("30-Minute Break Required")]
    Break30Minute = 3,

    [Description("70-Hour/8-Day Cycle Limit")]
    Cycle70Hour = 4,

    [Description("34-Hour Restart Required")]
    RestartRequired = 5,

    [Description("Form and Manner Violation")]
    FormAndMannerViolation = 6
}
