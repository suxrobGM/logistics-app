using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum SubscriptionStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,
    
    [Description("Inactive"), EnumMember(Value = "inactive")]
    Inactive,
    
    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled,
}
