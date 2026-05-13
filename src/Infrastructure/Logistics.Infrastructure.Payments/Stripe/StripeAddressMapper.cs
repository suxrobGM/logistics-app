using Logistics.Shared.Geo;
using Stripe;

using AddressValueObject = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Infrastructure.Payments.Stripe;

internal static class StripeAddressMapper
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
