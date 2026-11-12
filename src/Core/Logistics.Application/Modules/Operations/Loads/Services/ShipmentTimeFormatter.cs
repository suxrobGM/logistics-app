using System.Globalization;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>In the carrier's time zone.</summary>
internal sealed class ShipmentTimeFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
    private readonly TimeZoneInfo _timeZone;

    public ShipmentTimeFormatter(string timezone)
    {
        _timeZone = TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out var zone) ? zone : TimeZoneInfo.Utc;
    }

    public string? Format(DateTime? utc)
    {
        return utc is { } value ? ToLocal(value).ToString("MMM d, h:mm tt", Culture) : null;
    }

    public string FormatDate(DateTime utc)
    {
        return ToLocal(utc).ToString("MMMM d, yyyy", Culture);
    }

    /// <summary>"Nov 12, 8:00 AM - 2:00 PM", repeating the date only when the window spans days.</summary>
    public string? FormatWindow(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc is not { } from)
        {
            return Format(toUtc);
        }

        if (toUtc is not { } to || to == from)
        {
            return Format(from);
        }

        var localFrom = ToLocal(from);
        var localTo = ToLocal(to);
        var endFormat = localFrom.Date == localTo.Date ? "h:mm tt" : "MMM d, h:mm tt";
        return $"{localFrom.ToString("MMM d, h:mm tt", Culture)} - {localTo.ToString(endFormat, Culture)}";
    }

    private DateTime ToLocal(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _timeZone);
    }
}
