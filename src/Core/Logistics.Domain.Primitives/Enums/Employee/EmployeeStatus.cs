using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum EmployeeStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,

    [Description("On Leave"), EnumMember(Value = "on_leave")]
    OnLeave,

    [Description("Suspended"), EnumMember(Value = "suspended")]
    Suspended,

    [Description("Terminated"), EnumMember(Value = "terminated")]
    Terminated
}
