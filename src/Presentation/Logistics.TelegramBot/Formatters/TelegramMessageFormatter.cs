using System.Text;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logistics.TelegramBot.Formatters;

internal static partial class TelegramMessageFormatter
{
    private const int MaxMessageLength = 4096;

    public static string FormatLoads(IReadOnlyList<LoadDto> loads, int page, int totalPages)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Bold("Loads"));
        sb.AppendLine();

        if (loads.Count == 0)
        {
            sb.AppendLine(Escape("No loads found."));
            return sb.ToString();
        }

        foreach (var load in loads)
        {
            sb.AppendLine($"{Bold(Escape($"#{load.Number}"))} {Escape(load.Name ?? "")}");
            sb.AppendLine(Escape($"  Status: {load.Status.GetDescription()}"));
            sb.AppendLine(Escape($"  From: {FormatCity(load.OriginAddress)}"));
            sb.AppendLine(Escape($"  To: {FormatCity(load.DestinationAddress)}"));

            if (load.AssignedTruckNumber is not null)
                sb.AppendLine(Escape($"  Truck: {load.AssignedTruckNumber}"));

            sb.AppendLine(Escape($"  Cost: ${load.DeliveryCost:N2}"));
            sb.AppendLine();
        }

        if (totalPages > 1)
            sb.AppendLine(Escape($"Page {page}/{totalPages}"));

        return Truncate(sb.ToString());
    }

    public static string FormatTrucks(IReadOnlyList<TruckDto> trucks, int page, int totalPages)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Bold("Trucks"));
        sb.AppendLine();

        if (trucks.Count == 0)
        {
            sb.AppendLine(Escape("No trucks found."));
            return sb.ToString();
        }

        foreach (var truck in trucks)
        {
            sb.AppendLine($"{Bold(Escape(truck.Number ?? "N/A"))} \\- {Escape(truck.Status.GetDescription())}");
            sb.AppendLine(Escape($"  Type: {truck.Type.GetDescription()}"));

            if (truck.MainDriver is not null)
                sb.AppendLine(Escape($"  Driver: {truck.MainDriver.FullName ?? "N/A"}"));

            if (truck.CurrentAddress is not null)
                sb.AppendLine(Escape($"  Location: {FormatCity(truck.CurrentAddress)}"));

            sb.AppendLine();
        }

        if (totalPages > 1)
            sb.AppendLine(Escape($"Page {page}/{totalPages}"));

        return Truncate(sb.ToString());
    }

    public static string FormatTrips(IReadOnlyList<TripDto> trips, int page, int totalPages)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Bold("Trips"));
        sb.AppendLine();

        if (trips.Count == 0)
        {
            sb.AppendLine(Escape("No active trips found."));
            return sb.ToString();
        }

        foreach (var trip in trips)
        {
            sb.AppendLine($"{Bold(Escape($"#{trip.Number}"))} {Escape(trip.Name)}");
            sb.AppendLine(Escape($"  Status: {trip.Status.GetDescription()}"));
            sb.AppendLine(Escape($"  From: {FormatCity(trip.OriginAddress)}"));
            sb.AppendLine(Escape($"  To: {FormatCity(trip.DestinationAddress)}"));
            sb.AppendLine(Escape($"  Loads: {trip.LoadsCount}"));

            if (trip.TruckNumber is not null)
                sb.AppendLine(Escape($"  Truck: {trip.TruckNumber}"));

            sb.AppendLine();
        }

        if (totalPages > 1)
            sb.AppendLine(Escape($"Page {page}/{totalPages}"));

        return Truncate(sb.ToString());
    }

    public static string FormatHosStatus(IReadOnlyList<DriverHosStatusDto> statuses)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Bold("Driver HOS Status"));
        sb.AppendLine();

        if (statuses.Count == 0)
        {
            sb.AppendLine(Escape("No drivers with HOS data found."));
            return sb.ToString();
        }

        foreach (var status in statuses)
        {
            var violationMark = status.IsInViolation ? " \\[\\!\\]" : "";
            sb.AppendLine($"{Bold(Escape(status.EmployeeName ?? "Unknown"))}{violationMark}");
            sb.AppendLine(Escape($"  Status: {status.CurrentDutyStatusDisplay ?? "Unknown"}"));
            sb.AppendLine(Escape($"  Driving: {status.DrivingTimeRemainingDisplay ?? "N/A"}"));
            sb.AppendLine(Escape($"  On Duty: {status.OnDutyTimeRemainingDisplay ?? "N/A"}"));

            if (status.IsAvailableForDispatch)
                sb.AppendLine(Escape("  Available for dispatch"));

            sb.AppendLine();
        }

        return Truncate(sb.ToString());
    }

    public static InlineKeyboardMarkup? BuildPaginationKeyboard(string command, int page, int totalPages)
    {
        if (totalPages <= 1)
            return null;

        var buttons = new List<InlineKeyboardButton>();

        if (page > 1)
            buttons.Add(InlineKeyboardButton.WithCallbackData("<< Prev", $"{command}:page:{page - 1}"));

        if (page < totalPages)
            buttons.Add(InlineKeyboardButton.WithCallbackData("Next >>", $"{command}:page:{page + 1}"));

        return new InlineKeyboardMarkup([buttons]);
    }

    private static string FormatCity(Address address)
    {
        return $"{address.City}, {address.State}";
    }

    private static string Bold(string text)
    {
        return $"*{text}*";
    }

    private static string Truncate(string text)
    {
        return text.Length > MaxMessageLength
            ? string.Concat(text.AsSpan(0, MaxMessageLength - 20), "\n\\.\\.\\. \\(truncated\\)")
            : text;
    }
}
