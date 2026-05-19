using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.StripeConnect.Services;

public static class StripeObjectMapper
{
    public static SubscriptionStatus GetSubscriptionStatus(string stripeSubscriptionStatus)
    {
        return stripeSubscriptionStatus switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Cancelled,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            "trialing" => SubscriptionStatus.Trialing,
            "unpaid" => SubscriptionStatus.Unpaid,
            "paused" => SubscriptionStatus.Paused,
            _ => throw new ArgumentOutOfRangeException(nameof(stripeSubscriptionStatus), stripeSubscriptionStatus)
        };
    }

    public static PaymentStatus GetPaymentStatus(string stripePaymentStatus)
    {
        return stripePaymentStatus switch
        {
            "canceled" => PaymentStatus.Cancelled,
            "succeeded" => PaymentStatus.Paid,
            _ => throw new ArgumentOutOfRangeException(nameof(stripePaymentStatus), stripePaymentStatus)
        };
    }
}
