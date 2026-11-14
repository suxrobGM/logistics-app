using System.Globalization;

namespace Logistics.API.Serialization;

/// <summary>
/// Reads an ISO 8601 date as UTC whatever the host's time zone: one without an offset is labelled UTC,
/// one with an offset is converted. <see cref="DateTime.Parse(string)"/> would return local time instead.
/// </summary>
public static class UtcDateParser
{
    public static bool TryParse(string value, out DateTime utc) =>
        DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out utc);
}
