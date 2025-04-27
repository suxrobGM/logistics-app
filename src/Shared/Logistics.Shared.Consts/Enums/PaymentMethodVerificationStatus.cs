using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum PaymentMethodVerificationStatus
{
    [Description("Unverified"), EnumMember(Value = "unverified")]
    Unverified,
    
    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,

    [Description("Failed"), EnumMember(Value = "failed")]
    Failed,
    
    [Description("Verified"), EnumMember(Value = "verified")]
    Verified,
}