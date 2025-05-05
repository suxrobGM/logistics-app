using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;
/// <summary>
/// incomplete, incomplete_expired, trialing, active, past_due, canceled, unpaid, or paused
/// </summary>
public enum SubscriptionStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,
    
    // Subscription is removed from Stripe but not deleted from the database
    [Description("Incomplete"), EnumMember(Value = "incomplete")]
    Incomplete,
    
    [Description("Incomplete Expired"), EnumMember(Value = "incomplete_expired")]
    IncompleteExpired,
    
    [Description("Trialing"), EnumMember(Value = "trialing")]
    Trialing,
    
    [Description("Past Due"), EnumMember(Value = "past_due")]
    PastDue,
    
    // Subscription is cancelled in Stripe will be active until the end of the billing period
    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled,
    
    [Description("Unpaid"), EnumMember(Value = "unpaid")]
    Unpaid,
    
    [Description("Paused"), EnumMember(Value = "paused")]
    Paused,
}
