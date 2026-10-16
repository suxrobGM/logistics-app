using System.Text.RegularExpressions;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Geo;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

/// <summary>
///     Builds an <see cref="Address"/> from separate columns, filling any part they leave blank from a
///     single "full address" column parsed best-effort ("123 Main St, Dallas, TX 75201, US"). Validation
///     is the caller's job, so a half-parsed address is returned rather than guessed at.
/// </summary>
internal static partial class ImportAddressParser
{
    /// <summary>Spellings people type that are not the country's display name in <see cref="Countries"/>.</summary>
    private static readonly Dictionary<string, string> CountryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USA"] = "US", ["United States of America"] = "US", ["U.S."] = "US", ["U.S.A."] = "US",
        ["UK"] = "GB", ["Deutschland"] = "DE", ["Polska"] = "PL", ["Czech Republic"] = "CZ"
    };

    public static Address? Build(
        string? street, string? street2, string? city, string? state, string? zip, string? country,
        string? fullAddress, string defaultCountry)
    {
        if (!string.IsNullOrWhiteSpace(fullAddress))
        {
            var parsed = ParseFull(fullAddress);
            street ??= parsed.Street;
            city ??= parsed.City;
            state ??= parsed.State;
            zip ??= parsed.Zip;
            country ??= parsed.Country;
        }

        if (street is null && city is null && state is null && zip is null)
        {
            return null;
        }

        return new Address
        {
            Line1 = street ?? "",
            Line2 = street2,
            City = city ?? "",
            State = state ?? "",
            ZipCode = zip ?? "",
            Country = NormalizeCountry(country) ?? defaultCountry
        };
    }

    public static string? NormalizeCountry(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return null;
        }

        var trimmed = country.Trim();
        return CodeForName(trimmed) ?? trimmed.ToUpperInvariant();
    }

    private static (string? Street, string? City, string? State, string? Zip, string? Country) ParseFull(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        string? country = null;
        string? state = null;
        string? zip = null;

        if (parts.Count > 1 && IsCountry(parts[^1]))
        {
            country = NormalizeCountry(parts[^1]);
            parts.RemoveAt(parts.Count - 1);
        }

        if (parts.Count > 1)
        {
            var match = StateZip().Match(parts[^1]);
            if (match.Success)
            {
                state = match.Groups["state"].Value;
                zip = match.Groups["zip"].Success ? match.Groups["zip"].Value : null;
                parts.RemoveAt(parts.Count - 1);
            }
            else if (EuropeanZipCity().Match(parts[^1]) is { Success: true } eu)
            {
                // "10115 Berlin": EU addresses lead with the postcode and carry no state.
                zip = eu.Groups["zip"].Value;
                parts[^1] = eu.Groups["city"].Value;
            }
        }

        var city = parts.Count > 0 ? parts[^1] : null;
        var street = parts.Count > 1 ? string.Join(", ", parts[..^1]) : null;
        return (street, city, state, zip, country);
    }

    private static string? CodeForName(string name) =>
        CountryAliases.TryGetValue(name, out var code)
            ? code
            : Countries.AllCountries.FirstOrDefault(c => c.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Code;

    /// <summary>
    ///     Two-letter codes other than US are not treated as countries: "CA" is California far more often than Canada.
    /// </summary>
    private static bool IsCountry(string part) =>
        CodeForName(part) is not null || part.Equals("US", StringComparison.OrdinalIgnoreCase);

    /// <summary>"TX 75201", "TX 75201-1234", "ON M5V 2T6", or a bare "TX".</summary>
    [GeneratedRegex(@"^(?<state>[A-Za-z]{2,3})(?:\s+(?<zip>\d{5}(?:-\d{4})?|[A-Za-z]\d[A-Za-z]\s?\d[A-Za-z]\d))?$")]
    private static partial Regex StateZip();

    [GeneratedRegex(@"^(?<zip>\d{4,6}|\d{2}-\d{3})\s+(?<city>\D.+)$")]
    private static partial Regex EuropeanZipCity();
}
