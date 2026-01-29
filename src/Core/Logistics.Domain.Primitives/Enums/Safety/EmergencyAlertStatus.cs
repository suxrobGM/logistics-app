using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum EmergencyAlertStatus
{
    [Description("Active")] [EnumMember(Value = "active")]
    Active,

    [Description("Acknowledged")] [EnumMember(Value = "acknowledged")]
    Acknowledged,

    [Description("Dispatching Help")] [EnumMember(Value = "dispatching")]
    Dispatching,

    [Description("On Scene")] [EnumMember(Value = "on_scene")]
    OnScene,

    [Description("Resolved")] [EnumMember(Value = "resolved")]
    Resolved,

    [Description("False Alarm")] [EnumMember(Value = "false_alarm")]
    FalseAlarm
}
