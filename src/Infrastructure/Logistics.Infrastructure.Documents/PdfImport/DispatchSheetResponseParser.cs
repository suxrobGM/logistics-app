using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Documents.PdfImport;

/// <summary>
///     Turns the LLM's JSON response into a validated <see cref="ExtractedLoadDataDto"/>.
///     Tolerates markdown fences / surrounding prose and normalizes dates and currency.
/// </summary>
internal static class DispatchSheetResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static Result<ExtractedLoadDataDto> Parse(string llmText)
    {
        var parsed = Deserialize(llmText);
        if (parsed is null)
            return Result<ExtractedLoadDataDto>.Fail(
                "Could not extract structured load data from the PDF. The document may not be a dispatch sheet.");

        return Validate(MapToDto(parsed));
    }

    private static ParsedLoad? Deserialize(string text)
    {
        // The model is asked for raw JSON; defensively strip markdown fences / surrounding prose.
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            return null;

        try
        {
            return JsonSerializer.Deserialize<ParsedLoad>(text[start..(end + 1)], JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ExtractedLoadDataDto MapToDto(ParsedLoad parsed) => new()
    {
        OrderId = parsed.OrderId,
        Vehicles = [.. (parsed.Vehicles ?? []).Select(MapVehicle)],
        OriginAddress = MapAddress(parsed.OriginAddress),
        DestinationAddress = MapAddress(parsed.DestinationAddress),
        OriginContactName = parsed.OriginContactName,
        OriginContactPhone = parsed.OriginContactPhone,
        DestinationContactName = parsed.DestinationContactName,
        DestinationContactPhone = parsed.DestinationContactPhone,
        PickupDate = ParseDate(parsed.PickupDate),
        DeliveryDate = ParseDate(parsed.DeliveryDate),
        PaymentAmount = ParseAmount(parsed.PaymentAmount),
        ShipperName = parsed.ShipperName,
        ShipperPhone = parsed.ShipperPhone,
        ShipperEmail = parsed.ShipperEmail,
        SourceTemplate = "AI"
    };

    private static LoadVehicleInput MapVehicle(ParsedVehicle v) => new()
    {
        Vin = VinFormat.Normalize(v.Vin),
        Year = v.Year,
        Make = v.Make?.Trim(),
        Model = v.Model?.Trim(),
        Color = v.Color?.Trim(),
        LotNumber = v.LotNumber?.Trim(),
        IsOperable = v.Operable ?? true,
        Rate = ParseAmount(v.Price)
    };

    private static Address? MapAddress(ParsedAddress? a)
    {
        if (a is null || string.IsNullOrWhiteSpace(a.Line1))
            return null;

        return new Address
        {
            Line1 = a.Line1.Trim(),
            City = a.City?.Trim() ?? "",
            State = a.State?.Trim() ?? "",
            ZipCode = a.ZipCode?.Trim() ?? "",
            Country = string.IsNullOrWhiteSpace(a.Country) ? "USA" : a.Country.Trim()
        };
    }

    private static DateTime? ParseDate(string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date)
            ? date
            : null;

    private static decimal? ParseAmount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = value.Replace("$", "").Replace(",", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;
    }

    private static Result<ExtractedLoadDataDto> Validate(ExtractedLoadDataDto data)
    {
        var errors = new List<string>();

        if (!HasCoreFields(data.OriginAddress))
            errors.Add("Origin address could not be extracted");

        if (!HasCoreFields(data.DestinationAddress))
            errors.Add("Destination address could not be extracted");

        if (data.PaymentAmount is not > 0)
            errors.Add("Payment amount could not be extracted");

        return errors.Count > 0
            ? Result<ExtractedLoadDataDto>.Fail($"Missing required fields: {string.Join(", ", errors)}")
            : Result<ExtractedLoadDataDto>.Ok(data);
    }

    private static bool HasCoreFields(Address? address) =>
        address is not null &&
        !string.IsNullOrWhiteSpace(address.Line1) &&
        !string.IsNullOrWhiteSpace(address.City) &&
        !string.IsNullOrWhiteSpace(address.State);

    private sealed record ParsedLoad
    {
        public string? OrderId { get; init; }
        public List<ParsedVehicle>? Vehicles { get; init; }
        public ParsedAddress? OriginAddress { get; init; }
        public ParsedAddress? DestinationAddress { get; init; }
        public string? OriginContactName { get; init; }
        public string? OriginContactPhone { get; init; }
        public string? DestinationContactName { get; init; }
        public string? DestinationContactPhone { get; init; }
        public string? PickupDate { get; init; }
        public string? DeliveryDate { get; init; }
        public string? PaymentAmount { get; init; }
        public string? ShipperName { get; init; }
        public string? ShipperPhone { get; init; }
        public string? ShipperEmail { get; init; }
    }

    private sealed record ParsedVehicle
    {
        public string? Vin { get; init; }
        public int? Year { get; init; }
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? Color { get; init; }
        public string? LotNumber { get; init; }
        public bool? Operable { get; init; }
        public string? Price { get; init; }
    }

    private sealed record ParsedAddress
    {
        public string? Line1 { get; init; }
        public string? Line2 { get; init; }
        public string? City { get; init; }
        public string? State { get; init; }
        public string? ZipCode { get; init; }
        public string? Country { get; init; }
    }
}
