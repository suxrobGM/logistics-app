namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Best-effort mapping from a customer's billing country to Stripe's per-country tax-ID type
/// code. Falls back to <c>eu_vat</c> for unknown countries; Swiss/Norwegian VAT IDs are inferred
/// from prefix when the country code disagrees.
/// Reference: https://stripe.com/docs/billing/customer/tax-ids
/// </summary>
public static class StripeTaxIdTypes
{
    private static readonly Dictionary<string, string> ByCountry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GB"] = "gb_vat",
        ["US"] = "us_ein",
        ["AU"] = "au_abn",
        ["CA"] = "ca_gst_hst",
        ["JP"] = "jp_cn",
        ["IN"] = "in_gst",
        ["MX"] = "mx_rfc",
        ["BR"] = "br_cnpj",
        ["ZA"] = "za_vat",
        ["CH"] = "ch_vat",
        ["NO"] = "no_vat",
        ["NZ"] = "nz_gst"
    };

    public static string Infer(string country, string taxId)
    {
        if (ByCountry.TryGetValue(country, out var direct))
        {
            return direct;
        }

        // EU VAT IDs always start with the country prefix; Swiss/Norwegian sometimes too.
        if (taxId.StartsWith("CH", StringComparison.OrdinalIgnoreCase))
        {
            return "ch_vat";
        }
        if (taxId.StartsWith("NO", StringComparison.OrdinalIgnoreCase))
        {
            return "no_vat";
        }

        return "eu_vat";
    }
}
