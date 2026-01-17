using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvitationStatus
{
    [Description("Pending")] [EnumMember(Value = "pending")]
    Pending,

    [Description("Accepted")] [EnumMember(Value = "accepted")]
    Accepted,

    [Description("Expired")] [EnumMember(Value = "expired")]
    Expired,

    [Description("Cancelled")] [EnumMember(Value = "cancelled")]
    Cancelled
}
