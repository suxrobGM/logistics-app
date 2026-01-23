using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum ContactSubmissionStatus
{
    [Description("New")] [EnumMember(Value = "new")]
    New,

    [Description("In Progress")] [EnumMember(Value = "in-progress")]
    InProgress,

    [Description("Resolved")] [EnumMember(Value = "resolved")]
    Resolved,

    [Description("Closed")] [EnumMember(Value = "closed")]
    Closed,
}
