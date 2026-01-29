using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Maintenance;

public enum MaintenanceReminderStatus
{
    [Description("Pending")] [EnumMember(Value = "pending")]
    Pending,

    [Description("Sent")] [EnumMember(Value = "sent")]
    Sent,

    [Description("Acknowledged")] [EnumMember(Value = "acknowledged")]
    Acknowledged,

    [Description("Overdue")] [EnumMember(Value = "overdue")]
    Overdue,

    [Description("Completed")] [EnumMember(Value = "completed")]
    Completed
}
