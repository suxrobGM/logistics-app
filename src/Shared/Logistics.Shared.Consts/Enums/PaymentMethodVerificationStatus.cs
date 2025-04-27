using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum PaymentMethodVerificationStatus
{
    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,

    [Description("Verified"), EnumMember(Value = "verified")]
    Verified,

    [Description("Failed"), EnumMember(Value = "failed")]
    Failed,
}