using System.Globalization;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

/// <summary>
///     Lenient cell parsing for exports from other systems: currency symbols, thousands separators,
///     the tenant's date order, and enum spellings that differ from ours.
/// </summary>
internal static class ImportValueParser
{
    private const decimal MetersPerFoot = 0.3048m;

    private static readonly string[] IsoDateFormats =
        ["yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd"];

    private static readonly string[] UsDateFormats =
        ["M/d/yyyy", "M/d/yy", "M/d/yyyy H:mm", "M/d/yyyy h:mm tt", "M-d-yyyy", "MMM d, yyyy", "MMMM d, yyyy"];

    private static readonly string[] EuropeanDateFormats =
        ["d/M/yyyy", "d/M/yy", "d.M.yyyy", "d.M.yy", "d-M-yyyy", "d/M/yyyy H:mm", "d.M.yyyy H:mm", "d MMM yyyy"];

    /// <summary>
    ///     Parses "1,234.50", "$1 234,50" or "€ 900". With a single separator and three digits after it
    ///     ("1,234" / "1.234") it is read as thousands; otherwise the last separator is the decimal point.
    /// </summary>
    public static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string([.. value.Where(c => char.IsDigit(c) || c is '.' or ',' or '-')]);
        if (digits.Length == 0)
        {
            return null;
        }

        var lastDot = digits.LastIndexOf('.');
        var lastComma = digits.LastIndexOf(',');
        var decimalIndex = Math.Max(lastDot, lastComma);
        var separators = digits.Count(c => c is '.' or ',');

        if (decimalIndex >= 0 && separators == 1 && digits.Length - decimalIndex - 1 == 3)
        {
            decimalIndex = -1;
        }

        var normalized = decimalIndex < 0
            ? digits.Replace(".", "").Replace(",", "")
            : digits[..decimalIndex].Replace(".", "").Replace(",", "") + "." + digits[(decimalIndex + 1)..];

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    public static int? ParseInt(string? value)
    {
        var number = ParseDecimal(value);
        return number is { } n && n == decimal.Truncate(n) && n is >= int.MinValue and <= int.MaxValue
            ? (int)n
            : null;
    }

    /// <summary>
    ///     ISO dates always parse; slash dates follow the tenant's order, since 03/04/2026 is ambiguous.
    ///     The result is UTC with the time kept as written.
    /// </summary>
    public static DateTime? ParseDate(string? value, DateFormatType dateFormat)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string[] formats =
        [
            .. IsoDateFormats,
            .. dateFormat == DateFormatType.European ? EuropeanDateFormats : UsDateFormats
        ];

        return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces, out var date)
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : null;
    }

    /// <summary>
    ///     Matches the enum name, its display description, or one of <paramref name="aliases"/>,
    ///     all compared after header-style normalisation ("Step Deck", "step_deck", "STEPDECK").
    /// </summary>
    public static TEnum? ParseEnum<TEnum>(string? value, IReadOnlyDictionary<string, TEnum>? aliases = null)
        where TEnum : struct, Enum
    {
        var normalized = ImportHeaderMapper.Normalize(value);
        if (normalized.Length == 0)
        {
            return null;
        }

        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (ImportHeaderMapper.Normalize(candidate.ToString()) == normalized ||
                ImportHeaderMapper.Normalize(candidate.GetDescription()) == normalized)
            {
                return candidate;
            }
        }

        if (aliases is not null)
        {
            foreach (var (alias, candidate) in aliases)
            {
                if (ImportHeaderMapper.Normalize(alias) == normalized)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Converts an imported length to metres. A header naming the unit decides; otherwise US tenants
    ///     are read as feet and everyone else as metres.
    /// </summary>
    public static decimal ToMeters(decimal length, string? header, Region region)
    {
        var unit = header?.ToLowerInvariant() ?? "";
        var isFeet = unit.Contains("ft") || unit.Contains("feet") || unit.Contains("foot") || unit.Contains('\'');
        var isMeters = !isFeet && (unit.Contains("(m)") || unit.EndsWith(" m") || unit.Contains("meter") ||
                                   unit.Contains("metre"));

        var feet = isFeet || (!isMeters && region == Region.US);
        return feet ? Math.Round(length * MetersPerFoot, 2) : length;
    }
}
