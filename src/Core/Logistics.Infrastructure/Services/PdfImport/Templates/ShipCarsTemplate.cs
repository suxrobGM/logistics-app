using System.Globalization;
using System.Text.RegularExpressions;

using Logistics.Application.Services.PdfImport;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
/// Parser for Ship.Cars platform dispatch sheets.
/// </summary>
public sealed partial class ShipCarsTemplate : IDispatchSheetTemplate
{
    public string TemplateName => "Ship.Cars";

    public bool CanParse(string pdfText)
    {
        return pdfText.Contains("Ship.Cars", StringComparison.OrdinalIgnoreCase)
               || pdfText.Contains("Dispatch Contract #", StringComparison.OrdinalIgnoreCase);
    }

    public ExtractedLoadData Extract(string pdfText)
    {
        return new ExtractedLoadData
        {
            OrderId = ExtractOrderId(pdfText),
            VehicleYear = ExtractVehicleYear(pdfText),
            VehicleMake = ExtractVehicleMake(pdfText),
            VehicleModel = ExtractVehicleModel(pdfText),
            VehicleVin = ExtractVin(pdfText),
            VehicleType = ExtractVehicleType(pdfText),
            OriginAddress = ExtractOriginAddress(pdfText),
            DestinationAddress = ExtractDestinationAddress(pdfText),
            PickupDate = ExtractPickupDate(pdfText),
            DeliveryDate = ExtractDeliveryDate(pdfText),
            PaymentAmount = ExtractPaymentAmount(pdfText),
            ShipperName = ExtractShipperName(pdfText),
            SourceTemplate = TemplateName
        };
    }

    private static string? ExtractOrderId(string text)
    {
        var match = OrderIdRegex().Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static int? ExtractVehicleYear(string text)
    {
        var match = VehicleLineRegex().Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
        {
            return year;
        }
        return null;
    }

    private static string? ExtractVehicleMake(string text)
    {
        var match = VehicleLineRegex().Match(text);
        return match.Success ? match.Groups[2].Value.Trim() : null;
    }

    private static string? ExtractVehicleModel(string text)
    {
        var match = VehicleLineRegex().Match(text);
        return match.Success ? match.Groups[3].Value.Trim() : null;
    }

    private static string? ExtractVin(string text)
    {
        var match = VinRegex().Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractVehicleType(string text)
    {
        var match = VehicleTypeRegex().Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static ExtractedAddress? ExtractOriginAddress(string text)
    {
        // Look for ORIGIN section
        var originMatch = OriginSectionRegex().Match(text);
        if (!originMatch.Success) return null;

        var section = originMatch.Value;

        // Extract address line
        var addressMatch = OriginAddressRegex().Match(section);
        if (addressMatch.Success)
        {
            var fullAddress = addressMatch.Groups[1].Value.Trim();
            return ParseAddress(fullAddress);
        }

        return null;
    }

    private static ExtractedAddress? ExtractDestinationAddress(string text)
    {
        // Look for DESTINATION section
        var destMatch = DestinationSectionRegex().Match(text);
        if (!destMatch.Success) return null;

        var section = destMatch.Value;

        // Extract address line
        var addressMatch = DestAddressRegex().Match(section);
        if (addressMatch.Success)
        {
            var fullAddress = addressMatch.Groups[1].Value.Trim();
            return ParseAddress(fullAddress);
        }

        return null;
    }

    private static ExtractedAddress? ParseAddress(string fullAddress)
    {
        // Try to parse "123 Street, City, ST 12345" format
        var match = AddressParseRegex().Match(fullAddress);
        if (match.Success)
        {
            return new ExtractedAddress
            {
                Line1 = match.Groups[1].Value.Trim(),
                City = match.Groups[2].Value.Trim(),
                State = match.Groups[3].Value.Trim(),
                ZipCode = match.Groups[4].Value.Trim()
            };
        }

        // Fallback: just use the whole thing as Line1
        return new ExtractedAddress { Line1 = fullAddress };
    }

    private static DateTime? ExtractPickupDate(string text)
    {
        var match = PickupDateRegex().Match(text);
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var date))
        {
            return date;
        }
        return null;
    }

    private static DateTime? ExtractDeliveryDate(string text)
    {
        var match = DeliveryDateRegex().Match(text);
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var date))
        {
            return date;
        }
        return null;
    }

    private static decimal? ExtractPaymentAmount(string text)
    {
        var match = PaymentRegex().Match(text);
        if (match.Success)
        {
            var amountStr = match.Groups[1].Value.Replace(",", "");
            if (decimal.TryParse(amountStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out var amount))
            {
                return amount;
            }
        }
        return null;
    }

    private static string? ExtractShipperName(string text)
    {
        var match = BrokerNameRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // Regex patterns
    [GeneratedRegex(@"Dispatch Contract\s*#(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex OrderIdRegex();

    [GeneratedRegex(@"(\d{4})\s+([\w]+)\s+([\w]+)\s+[A-HJ-NPR-Z0-9]{17}", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleLineRegex();

    [GeneratedRegex(@"([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"Type\s+(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    [GeneratedRegex(@"ORIGIN.*?(?=DESTINATION|VEHICLE|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex OriginSectionRegex();

    [GeneratedRegex(@"DESTINATION.*?(?=VEHICLE|TIMEFRAMES|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex DestinationSectionRegex();

    [GeneratedRegex(@"Address\s+(.+?)(?:\nPhone|\n\()", RegexOptions.IgnoreCase)]
    private static partial Regex OriginAddressRegex();

    [GeneratedRegex(@"Address\s+(.+?)(?:\nPhone|\n\()", RegexOptions.IgnoreCase)]
    private static partial Regex DestAddressRegex();

    [GeneratedRegex(@"(.+),\s*([^,]+),\s*([A-Z]{2})\s*(\d{5})", RegexOptions.IgnoreCase)]
    private static partial Regex AddressParseRegex();

    [GeneratedRegex(@"Pickup\s+Exactly\s+(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"Delivery\s+Exactly\s+(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    [GeneratedRegex(@"Total Carrier Pay\s*\$?([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    [GeneratedRegex(@"BROKER\s+([^\n]+?)(?:\s+USDOT|\n)", RegexOptions.IgnoreCase)]
    private static partial Regex BrokerNameRegex();
}
