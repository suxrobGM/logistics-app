using System.Globalization;
using System.Text.RegularExpressions;

using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

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

    public ExtractedLoadDataDto Extract(string pdfText)
    {
        return new ExtractedLoadDataDto
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

    private static Address? ExtractOriginAddress(string text)
    {
        // Ship.Cars PDF format (concatenated without line breaks):
        // "ORIGINPickup fromCompany...Address 555 Pacific Ave, Santa Cruz, CA 95060Phone..."
        // Find the ORIGIN section and extract address between "Address" and "Phone"

        var originIdx = text.IndexOf("ORIGIN", StringComparison.OrdinalIgnoreCase);
        var destIdx = text.IndexOf("DESTINATION", StringComparison.OrdinalIgnoreCase);

        if (originIdx < 0 || destIdx < 0 || destIdx <= originIdx)
        {
            return null;
        }

        // Get the ORIGIN section (text between ORIGIN and DESTINATION)
        var originSection = text[originIdx..destIdx];

        // Find "Address" followed by the address, ending at "Phone"
        var addressIdx = originSection.IndexOf("Address", StringComparison.OrdinalIgnoreCase);
        if (addressIdx < 0)
        {
            return null;
        }

        var phoneIdx = originSection.IndexOf("Phone", addressIdx + 7, StringComparison.OrdinalIgnoreCase);
        if (phoneIdx < 0)
        {
            return null;
        }

        var addressText = originSection[(addressIdx + 7)..phoneIdx].Trim();
        return ParseAddressFromText(addressText);
    }

    private static Address? ExtractDestinationAddress(string text)
    {
        // Find DESTINATION section
        var destIdx = text.IndexOf("DESTINATION", StringComparison.OrdinalIgnoreCase);
        if (destIdx < 0)
        {
            return null;
        }

        // Find the end of DESTINATION section (at VEHICLE INFORMATION or similar)
        var vehicleIdx = text.IndexOf("VEHICLE INFORMATION", destIdx, StringComparison.OrdinalIgnoreCase);
        var vipIdx = text.IndexOf("VIP ORDER", destIdx, StringComparison.OrdinalIgnoreCase);

        var endIdx = text.Length;
        if (vehicleIdx > 0 && vehicleIdx < endIdx) endIdx = vehicleIdx;
        if (vipIdx > 0 && vipIdx < endIdx) endIdx = vipIdx;

        var destSection = text[destIdx..endIdx];

        // Find "Address" followed by the address, ending at "Phone"
        var addressIdx = destSection.IndexOf("Address", StringComparison.OrdinalIgnoreCase);
        if (addressIdx < 0)
        {
            return null;
        }

        var phoneIdx = destSection.IndexOf("Phone", addressIdx + 7, StringComparison.OrdinalIgnoreCase);
        if (phoneIdx < 0)
        {
            return null;
        }

        var addressText = destSection[(addressIdx + 7)..phoneIdx].Trim();
        return ParseAddressFromText(addressText);
    }

    private static Address? ParseAddressFromText(string addressText)
    {
        // Parse address like "555 Pacific Ave, Santa Cruz, CA 95060"
        // or "10208 Loving Trail Dr, Frisco, TX 75035"
        var match = AddressParseRegex().Match(addressText);
        if (match.Success)
        {
            return new Address
            {
                Line1 = match.Groups[1].Value.Trim(),
                City = match.Groups[2].Value.Trim(),
                State = match.Groups[3].Value.Trim(),
                ZipCode = match.Groups[4].Value.Trim(),
                Country = "USA"
            };
        }

        // Try alternative pattern for concatenated text without commas
        // "555 Pacific AveSanta Cruz CA 95060"
        var altMatch = ConcatenatedAddressRegex().Match(addressText);
        if (altMatch.Success)
        {
            return new Address
            {
                Line1 = altMatch.Groups[1].Value.Trim(),
                City = altMatch.Groups[2].Value.Trim(),
                State = altMatch.Groups[3].Value.Trim(),
                ZipCode = altMatch.Groups[4].Value.Trim(),
                Country = "USA"
            };
        }

        return null;
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

    // Vehicle type can appear in various formats - "Type Sedan" or just "Sedan" in vehicle info
    [GeneratedRegex(@"\b(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    // Address with commas: "555 Pacific Ave, Santa Cruz, CA 95060"
    [GeneratedRegex(@"(.+),\s*([^,]+),\s*([A-Z]{2})\s*(\d{5}(?:-\d{4})?)", RegexOptions.IgnoreCase)]
    private static partial Regex AddressParseRegex();

    // Address without commas (concatenated): "555 Pacific AveSanta Cruz CA 95060"
    [GeneratedRegex(@"(\d+[^,]*?(?:Ave|Avenue|St|Street|Rd|Road|Blvd|Boulevard|Dr|Drive|Ln|Lane|Way|Ct|Court|Pl|Place|Cir|Circle|Trail)[^,]*?),?\s*([A-Za-z\s]+),?\s*([A-Z]{2})\s+(\d{5}(?:-\d{4})?)", RegexOptions.IgnoreCase)]
    private static partial Regex ConcatenatedAddressRegex();

    [GeneratedRegex(@"Pickup\s+Exactly\s+(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"Delivery\s+Exactly\s+(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    // Payment amount appears after "Total Carrier Pay" but may have intermediate text like "Broker to CarrierShip Via"
    [GeneratedRegex(@"Total Carrier Pay[^\d$]*\$?([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    // Broker name: "BROKER\nMontway Auto Transport USDOT#..." or concatenated "BROKERMontway Auto Transport USDOT#..."
    [GeneratedRegex(@"BROKER\s*([A-Za-z][A-Za-z\s]+?)(?:\s+USDOT|USDOT)", RegexOptions.IgnoreCase)]
    private static partial Regex BrokerNameRegex();
}
