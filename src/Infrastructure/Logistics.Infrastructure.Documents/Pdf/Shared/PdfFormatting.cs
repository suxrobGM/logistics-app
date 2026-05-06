using System.Globalization;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.Services.Pdf.Shared;

/// <summary>
/// Display formatters reused across invoice and pay-stub PDFs. Currency is rendered as
/// <c>1,234.56 USD</c> rather than a hardcoded "$" so multi-currency tenants get the right ISO code.
/// </summary>
internal static class PdfFormatting
{
    public static string Money(decimal amount, string currency) =>
        $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {currency}";

    /// <summary>"19%", "21.5%", or "—" when the rate is zero.</summary>
    public static string Rate(decimal percent) =>
        percent <= 0m ? "—" : percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    /// <summary>"Amsterdam, NH 1011 AB, NL" or "Amsterdam 1011 AB, NL" when state is missing.</summary>
    public static string CityLine(Address address)
    {
        var state = string.IsNullOrEmpty(address.State) ? "" : $", {address.State}";
        return $"{address.City}{state} {address.ZipCode}, {address.Country}";
    }

    /// <summary>Region-aware tax label: "VAT" / "Sales tax" / "Tax".</summary>
    public static string TaxLabel(Region? region) => region switch
    {
        Region.EU => "VAT",
        Region.US => "Sales tax",
        _ => "Tax"
    };

    /// <summary>Humanize an enum (uses [Description] attribute or PascalCase splitter).</summary>
    public static string Display(Enum value) => value.GetDescription();
}
