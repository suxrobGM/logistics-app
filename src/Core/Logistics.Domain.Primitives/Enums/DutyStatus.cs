using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum DutyStatus
{
    [Description("Off Duty")] [EnumMember(Value = "off_duty")]
    OffDuty = 0,

    [Description("Sleeper Berth")] [EnumMember(Value = "sleeper_berth")]
    SleeperBerth = 1,

    [Description("Driving")] [EnumMember(Value = "driving")]
    Driving = 2,

    [Description("On Duty (Not Driving)")] [EnumMember(Value = "on_duty_not_driving")]
    OnDutyNotDriving = 3,

    [Description("Yard Move")] [EnumMember(Value = "yard_move")]
    YardMove = 4,

    [Description("Personal Conveyance")] [EnumMember(Value = "personal_conveyance")]
    PersonalConveyance = 5
}
