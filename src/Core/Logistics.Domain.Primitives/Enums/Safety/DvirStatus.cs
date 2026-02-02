using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DvirStatus
{
    [Description("Draft")] [EnumMember(Value = "draft")]
    Draft,

    [Description("Submitted")] [EnumMember(Value = "submitted")]
    Submitted,

    [Description("Reviewed")] [EnumMember(Value = "reviewed")]
    Reviewed,

    [Description("Requires Repair")] [EnumMember(Value = "requires_repair")]
    RequiresRepair,

    [Description("Cleared")] [EnumMember(Value = "cleared")]
    Cleared,

    [Description("Rejected")] [EnumMember(Value = "rejected")]
    Rejected
}
