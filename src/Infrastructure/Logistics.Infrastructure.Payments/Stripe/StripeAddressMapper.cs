using Logistics.Application.Abstractions.Payments;
using Logistics.Shared.Geo;
using Stripe;

using AddressValueObject = Logistics.Domain.Primitives.ValueObjects.Address;
using StripeAddress = Stripe.Address;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeAddressMapper : IStripeAddressMapper
{
    public AddressValueObject ToAddress(StripeAddress stripeAddress)
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

    public AddressValueObject ToAddress(AddressOptions addressOptions)
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
}

internal static class StripeAddressMapperExtensions
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

        if (country.Length == 2)
        {
            return country.ToUpperInvariant();
        }

        return Countries.FindCountry(country)?.Code;
    }
}
