using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Geo;
using Stripe;

using AddressValueObject = Logistics.Domain.Primitives.ValueObjects.Address;
using StripeAddress = Stripe.Address;

namespace Logistics.Application.Services;

public static class StripeObjectMapper
{
    public static AddressOptions ToStripeAddressOptions(this AddressValueObject address)
    {
        return new AddressOptions
        {
            City = address.City,
            Country = GetCountryCode(address.Country),
            Line1 = address.Line1,
            Line2 = address.Line2,
            PostalCode = address.ZipCode,
            State = address.State
        };
    }

    private static string? GetCountryCode(string? country)
    {
        if (string.IsNullOrEmpty(country))
        {
            return null;
        }

        // If already a 2-letter code, return as-is
        if (country.Length == 2)
        {
            return country.ToUpperInvariant();
        }

        // Look up the country by name and return the ISO code
        return Countries.FindCountry(country)?.Code;
    }

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
