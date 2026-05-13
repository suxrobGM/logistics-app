using Logistics.Domain.Primitives.Enums;
using Stripe;

using AddressValueObject = Logistics.Domain.Primitives.ValueObjects.Address;
using StripeAddress = Stripe.Address;

namespace Logistics.Application.Services;

public static class StripeObjectMapper
{
    public static AddressValueObject ToAddressEntity(this StripeAddress stripeAddress)
    {
        return new AddressValueObject
        {
            City = stripeAddress.City,
            Country = stripeAddress.Country,
            Line1 = stripeAddress.Line1,
            Line2 = stripeAddress.Line2,
            ZipCode = stripeAddress.PostalCode,
            State = stripeAddress.State
        };
    }

    public static AddressValueObject ToAddressEntity(this AddressOptions addressOptions)
    {
        return new AddressValueObject
        {
            City = addressOptions.City,
            Country = addressOptions.Country,
            Line1 = addressOptions.Line1,
            Line2 = addressOptions.Line2,
            ZipCode = addressOptions.PostalCode,
            State = addressOptions.State
        };
    }

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
