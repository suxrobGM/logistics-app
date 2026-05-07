using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Resolves the Stripe Connect <see cref="AccountCapabilitiesOptions"/> set requested for a connected
/// account based on the account's ISO 3166-1 alpha-2 country code.
/// </summary>
internal static class StripeCapabilities
{
    public static AccountCapabilitiesOptions ForCountry(string? countryCode)
    {
        var country = countryCode?.ToUpperInvariant();

        var capabilities = new AccountCapabilitiesOptions
        {
            CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
        };

        switch (country)
        {
            case "US":
            case "CA":
                capabilities.UsBankAccountAchPayments =
                    new AccountCapabilitiesUsBankAccountAchPaymentsOptions { Requested = true };
                return capabilities;

            case "GB":
                capabilities.BacsDebitPayments =
                    new AccountCapabilitiesBacsDebitPaymentsOptions { Requested = true };
                return capabilities;

            // Inside geographic Europe but outside the SEPA Direct Debit zone — keep
            // card-only to avoid requesting capabilities Stripe will reject.
            case "CH":
            case "NO":
            case "IS":
            case "LI":
                return capabilities;
        }

        if (!RegionCountries.IsAllowed(Region.EU, country))
        {
            return capabilities;
        }

        capabilities.SepaDebitPayments =
            new AccountCapabilitiesSepaDebitPaymentsOptions { Requested = true };

        switch (country)
        {
            case "DE":
            case "AT":
                capabilities.GiropayPayments =
                    new AccountCapabilitiesGiropayPaymentsOptions { Requested = true };
                capabilities.SofortPayments =
                    new AccountCapabilitiesSofortPaymentsOptions { Requested = true };
                break;
            case "NL":
                capabilities.IdealPayments =
                    new AccountCapabilitiesIdealPaymentsOptions { Requested = true };
                break;
            case "BE":
                capabilities.BancontactPayments =
                    new AccountCapabilitiesBancontactPaymentsOptions { Requested = true };
                break;
        }

        return capabilities;
    }
}
