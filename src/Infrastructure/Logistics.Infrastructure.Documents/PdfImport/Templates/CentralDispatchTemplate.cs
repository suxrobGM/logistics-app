using System.Globalization;
using System.Text.RegularExpressions;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
///     Parser for CentralDispatch platform dispatch sheets.
/// </summary>
internal sealed partial class CentralDispatchTemplate : IDispatchSheetTemplate
{
    public string TemplateName => "CentralDispatch";

    public bool CanParse(string pdfText)
    {
        return pdfText.Contains("CentralDispatch", StringComparison.OrdinalIgnoreCase)
               || (pdfText.Contains("Dispatch Sheet", StringComparison.OrdinalIgnoreCase)
                   && pdfText.Contains("Load ID", StringComparison.OrdinalIgnoreCase));
    }

    public ExtractedLoadDataDto Extract(string pdfText)
    {
        var (originAddress, originContactName) = ExtractOriginAddress(pdfText);
        var (destinationAddress, destinationContactName) = ExtractDestinationAddress(pdfText);

        return new ExtractedLoadDataDto
        {
            OrderId = ExtractOrderId(pdfText),
            VehicleYear = ExtractVehicleYear(pdfText),
            VehicleMake = ExtractVehicleMake(pdfText),
            VehicleModel = ExtractVehicleModel(pdfText),
            VehicleVin = ExtractVin(pdfText),
            VehicleType = ExtractVehicleType(pdfText),
            OriginAddress = originAddress,
            DestinationAddress = destinationAddress,
            OriginContactName = originContactName,
            DestinationContactName = destinationContactName,
            PickupDate = ExtractPickupDate(pdfText),
            DeliveryDate = ExtractDeliveryDate(pdfText),
            PaymentAmount = ExtractPaymentAmount(pdfText),
            ShipperName = ExtractShipperName(pdfText),
            SourceTemplate = TemplateName
        };
    }

    private static string? ExtractOrderId(string text)
    {
        var match = LoadIdRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static int? ExtractVehicleYear(string text)
    {
        var match = VehicleYearMakeModelRegex().Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
        {
            return year;
        }

        return null;
    }

    private static string? ExtractVehicleMake(string text)
    {
        var match = VehicleYearMakeModelRegex().Match(text);
        return match.Success ? match.Groups[2].Value.Trim() : null;
    }

    private static string? ExtractVehicleModel(string text)
    {
        var match = VehicleYearMakeModelRegex().Match(text);
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

    private static (Address? Address, string? ContactName) ExtractOriginAddress(string text)
    {
        // CentralDispatch PDF format - "Origin Info" and "Destination Info" are column headers on same line
        // Concatenated text: "Origin Info Destination InfoOriginManheim Pennsylania1190 Lancaster RoadManheim, PA 17545Contact Info"
        // Need to find "Origin" label (not "Origin Info") and extract until first "Contact Info"

        // Find all occurrences and use context to determine which is the actual label
        var originInfoIdx = text.IndexOf("Origin Info", StringComparison.OrdinalIgnoreCase);
        if (originInfoIdx < 0)
        {
            return (null, null);
        }

        // Find "Origin" that appears after any headers (look for standalone "Origin" followed by location data)
        // Skip past "Origin Info" and "Destination Info" headers
        var searchStart = originInfoIdx + 11;
        var destInfoIdx = text.IndexOf("Destination Info", searchStart, StringComparison.OrdinalIgnoreCase);
        if (destInfoIdx > 0 && destInfoIdx < searchStart + 20)
        {
            searchStart = destInfoIdx + 16; // Skip past "Destination Info" header too
        }

        // Now find "Origin" label (the actual data label, not the header)
        var originLabelIdx = text.IndexOf("Origin", searchStart, StringComparison.OrdinalIgnoreCase);
        if (originLabelIdx < 0)
        {
            return (null, null);
        }

        // Find "Contact Info" or "Destination" which ends the origin address block
        var contactIdx = text.IndexOf("Contact Info", originLabelIdx + 6, StringComparison.OrdinalIgnoreCase);
        var destLabelIdx = text.IndexOf("Destination", originLabelIdx + 6, StringComparison.OrdinalIgnoreCase);

        // Use the earlier of Contact Info or Destination as the end marker
        var endIdx = text.Length;
        if (contactIdx > 0 && contactIdx < endIdx)
        {
            endIdx = contactIdx;
        }

        if (destLabelIdx > 0 && destLabelIdx < endIdx)
        {
            endIdx = destLabelIdx;
        }

        // Extract the text between "Origin" label and end marker
        var addressBlock = text[(originLabelIdx + 6)..endIdx].Trim();

        return ParseAddressFromConcatenatedBlock(addressBlock);
    }

    private static (Address? Address, string? ContactName) ExtractDestinationAddress(string text)
    {
        // Find "Destination" label (not "Destination Info" header)
        // Look for "Destination" that's followed by location data (after the Origin section)

        var originInfoIdx = text.IndexOf("Origin Info", StringComparison.OrdinalIgnoreCase);
        if (originInfoIdx < 0)
        {
            return (null, null);
        }

        // Skip past headers to find data labels
        var searchStart = originInfoIdx + 11;
        var destInfoIdx = text.IndexOf("Destination Info", searchStart, StringComparison.OrdinalIgnoreCase);
        if (destInfoIdx > 0 && destInfoIdx < searchStart + 20)
        {
            searchStart = destInfoIdx + 16;
        }

        // Find the first "Origin" label
        var originLabelIdx = text.IndexOf("Origin", searchStart, StringComparison.OrdinalIgnoreCase);
        if (originLabelIdx < 0)
        {
            return (null, null);
        }

        // Find "Destination" label after the Origin label
        var destLabelIdx = text.IndexOf("Destination", originLabelIdx + 6, StringComparison.OrdinalIgnoreCase);
        if (destLabelIdx < 0)
        {
            return (null, null);
        }

        // Find end of destination block (Contact Info, Location Type, Dates, etc.)
        var endMarkers = new[]
        {
            "Contact Info", "Location Type", "Dates", "Load Info", "Vehicle Info", "Additional Info"
        };
        var endIdx = text.Length;

        foreach (var marker in endMarkers)
        {
            var markerIdx = text.IndexOf(marker, destLabelIdx + 11, StringComparison.OrdinalIgnoreCase);
            if (markerIdx > 0 && markerIdx < endIdx)
            {
                endIdx = markerIdx;
            }
        }

        // Extract the text between "Destination" label and end marker
        var addressBlock = text[(destLabelIdx + 11)..endIdx].Trim();

        return ParseAddressFromConcatenatedBlock(addressBlock);
    }

    private static (Address? Address, string? ContactName) ParseAddressFromConcatenatedBlock(string block)
    {
        // Block format: "Manheim Pennsylania1190 Lancaster RoadManheim, PA 17545"
        // or "AllServic Statn133 Avenue SBrooklyn, NY 11223"
        // The contact name is the first part before the street number

        // Try to find address pattern: street number + street + city, state zip
        var addressMatch = ConcatenatedAddressRegex().Match(block);
        if (addressMatch.Success)
        {
            var street = addressMatch.Groups[1].Value.Trim();
            var city = addressMatch.Groups[2].Value.Trim();
            var state = addressMatch.Groups[3].Value.Trim();
            var zip = addressMatch.Groups[4].Value.Trim();

            // Contact name is text before the street number
            var streetStartIdx = block.IndexOf(street, StringComparison.OrdinalIgnoreCase);
            var contactName = streetStartIdx > 0 ? block[..streetStartIdx].Trim() : null;

            var address = new Address
            {
                Line1 = street,
                City = city,
                State = state,
                ZipCode = zip,
                Country = "USA"
            };
            return (address, contactName);
        }

        return (null, null);
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
        var match = ShipperNameRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // Regex patterns
    // Load ID appears before "Total Price" in concatenated text
    [GeneratedRegex(@"Load ID\s*(.+?)(?:Total|$)", RegexOptions.IgnoreCase)]
    private static partial Regex LoadIdRegex();

    [GeneratedRegex(@"Vehicle Year/Make/Model\s+(\d{4})\s+([\w]+)\s+([\w]+)", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleYearMakeModelRegex();

    [GeneratedRegex(@"VIN\s+([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"Vehicle Type\s+(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    // Address pattern for concatenated text: "1190 Lancaster RoadManheim, PA 17545" or "133 Avenue SBrooklyn, NY 11223"
    // Handles directional suffixes like "S", "N", "E", "W" that may be concatenated with city name
    // Captures: street (starting with number + street type + optional directional), city, state, zip
    [GeneratedRegex(
        @"(\d+[^,]*?(?:Road|Rd|Street|St|Avenue|Ave|Drive|Dr|Lane|Ln|Boulevard|Blvd|Way|Court|Ct|Place|Pl|Circle|Cir|Trail)(?:\s*[NSEW](?=[A-Z]))?)([A-Za-z][A-Za-z\s]*?),\s*([A-Z]{2})\s+(\d{5})",
        RegexOptions.IgnoreCase)]
    private static partial Regex ConcatenatedAddressRegex();

    [GeneratedRegex(@"Scheduled Pick-Up\s+(?:Exactly\s+)?(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"Scheduled Delivery\s+(?:Exactly\s+)?(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    [GeneratedRegex(@"Total Price\s*\$?([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    // Shipper name appears after "Shipper" label until the next section or number
    [GeneratedRegex(@"Shipper\s*([A-Za-z][A-Za-z\s]+?)(?:\d|Contact|Special|$)", RegexOptions.IgnoreCase)]
    private static partial Regex ShipperNameRegex();
}
