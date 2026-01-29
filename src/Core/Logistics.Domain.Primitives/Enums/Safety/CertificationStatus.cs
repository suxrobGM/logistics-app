using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum CertificationStatus
{
    [Description("Active")] [EnumMember(Value = "active")]
    Active,

    [Description("Expiring Soon")] [EnumMember(Value = "expiring_soon")]
    ExpiringSoon,

    [Description("Expired")] [EnumMember(Value = "expired")]
    Expired,

    [Description("Revoked")] [EnumMember(Value = "revoked")]
    Revoked,

    [Description("Suspended")] [EnumMember(Value = "suspended")]
    Suspended
}
