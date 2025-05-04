using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum SubscriptionStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,
    
    // Subscription is removed from Stripe but not deleted from the database
    [Description("Inactive"), EnumMember(Value = "inactive")]
    Inactive,
    
    // Subscription is cancelled in Stripe will be active until the end of the billing period
    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled,
}
