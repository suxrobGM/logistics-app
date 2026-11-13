namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// incomplete, incomplete_expired, trialing, active, past_due, canceled, unpaid, or paused
/// </summary>
public enum SubscriptionStatus
{
    Active,

    // Subscription is removed from Stripe but not deleted from the database
    Incomplete,
    IncompleteExpired,
    Trialing,
    PastDue,

    // Subscription is cancelled in Stripe will be active until the end of the billing period
    Cancelled,
    Unpaid,
    Paused
}
