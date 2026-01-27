using System.Globalization;
using System.Text.RegularExpressions;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
///     Parser for Super Dispatch platform dispatch sheets.
/// </summary>
internal sealed partial class SuperDispatchTemplate : IDispatchSheetTemplate
{
    public string TemplateName => "Super Dispatch";

    public bool CanParse(string pdfText)
    {
        return pdfText.Contains("Powered by Super Dispatch", StringComparison.OrdinalIgnoreCase)
               || pdfText.Contains("Super Dispatch", StringComparison.OrdinalIgnoreCase);
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
        var match = OrderIdRegex().Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static int? ExtractVehicleYear(string text)
    {
        var match = VehicleInfoRegex().Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
        {
            return year;
        }

        return null;
    }

    private static string? ExtractVehicleMake(string text)
    {
        var match = VehicleInfoRegex().Match(text);
        return match.Success ? match.Groups[2].Value.Trim() : null;
    }

    private static string? ExtractVehicleModel(string text)
    {
        var match = VehicleInfoRegex().Match(text);
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
        // The PDF text is concatenated without proper line breaks
        // Format: "Pickup InformationDelivery InformationName:  contact nameLocation...Street AddressCity, ST ZIPPhone:"
        // We need to find the first Name: after "Pickup Information" and extract until Phone:

        var pickupIdx = text.IndexOf("Pickup Information", StringComparison.OrdinalIgnoreCase);
        var deliveryIdx = text.IndexOf("Delivery Information", StringComparison.OrdinalIgnoreCase);

        if (pickupIdx < 0 || deliveryIdx < 0)
        {
            return (null, null);
        }

        // Find the first "Name:" after Delivery Information header (which comes right after Pickup Information in the concatenated text)
        // The pickup address is the FIRST Name: block, delivery is the SECOND
        var searchStart = deliveryIdx + "Delivery Information".Length;
        var firstNameIdx = text.IndexOf("Name:", searchStart, StringComparison.OrdinalIgnoreCase);

        if (firstNameIdx < 0)
        {
            return (null, null);
        }

        // Find where this address block ends (at "Phone:" or "Mobile:")
        var firstPhoneIdx = text.IndexOf("Phone:", firstNameIdx + 5, StringComparison.OrdinalIgnoreCase);
        if (firstPhoneIdx < 0)
        {
            return (null, null);
        }

        var pickupSection = text[firstNameIdx..firstPhoneIdx];
        return ExtractAddressFromConcatenatedSection(pickupSection);
    }

    private static (Address? Address, string? ContactName) ExtractDestinationAddress(string text)
    {
        // Find the second Name: block (delivery address)
        var deliveryIdx = text.IndexOf("Delivery Information", StringComparison.OrdinalIgnoreCase);
        if (deliveryIdx < 0)
        {
            return (null, null);
        }

        var searchStart = deliveryIdx + "Delivery Information".Length;
        var firstNameIdx = text.IndexOf("Name:", searchStart, StringComparison.OrdinalIgnoreCase);
        if (firstNameIdx < 0)
        {
            return (null, null);
        }

        // Skip past the first Phone:/Mobile: block to find the second Name:
        var firstPhoneIdx = text.IndexOf("Phone:", firstNameIdx + 5, StringComparison.OrdinalIgnoreCase);
        if (firstPhoneIdx < 0)
        {
            return (null, null);
        }

        // Look for "Mobile:" after first Phone:
        var firstMobileIdx = text.IndexOf("Mobile:", firstPhoneIdx, StringComparison.OrdinalIgnoreCase);
        var secondNameStart = firstMobileIdx > 0 ? firstMobileIdx + 7 : firstPhoneIdx + 6;

        var secondNameIdx = text.IndexOf("Name:", secondNameStart, StringComparison.OrdinalIgnoreCase);
        if (secondNameIdx < 0)
        {
            return (null, null);
        }

        // Find where the second address block ends
        var secondPhoneIdx = text.IndexOf("Phone:", secondNameIdx + 5, StringComparison.OrdinalIgnoreCase);
        if (secondPhoneIdx < 0)
        {
            // Try to find end at "* -" or "REVISIONS"
            secondPhoneIdx = text.IndexOf("* -", secondNameIdx, StringComparison.OrdinalIgnoreCase);
            if (secondPhoneIdx < 0)
            {
                secondPhoneIdx = text.IndexOf("REVISIONS", secondNameIdx, StringComparison.OrdinalIgnoreCase);
            }
        }

        if (secondPhoneIdx < 0)
        {
            return (null, null);
        }

        var deliverySection = text[secondNameIdx..secondPhoneIdx];
        return ExtractAddressFromConcatenatedSection(deliverySection);
    }

    private static (Address? Address, string? ContactName) ExtractAddressFromConcatenatedSection(string section)
    {
        // Section format: "Name:  contact name...Street AddressCity, ST ZIP"
        // Extract contact name after "Name:"
        var nameMatch = ContactNameRegex().Match(section);
        var contactName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : null;

        // Extract address: look for pattern "number...street...City, ST ZIP"
        // The address pattern: street number + text + City, STATE ZIP
        var addressMatch = ConcatenatedAddressRegex().Match(section);
        if (addressMatch.Success)
        {
            var address = new Address
            {
                Line1 = addressMatch.Groups[1].Value.Trim().TrimEnd(','),
                City = addressMatch.Groups[2].Value.Trim(),
                State = addressMatch.Groups[3].Value.Trim(),
                ZipCode = addressMatch.Groups[4].Value.Trim(),
                Country = "USA"
            };
            return (address, contactName);
        }

        return (null, contactName);
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
        // Look for shipper name at the beginning - text before "Shipper"
        var match = ShipperNameRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // Regex patterns
    [GeneratedRegex(@"Order ID:\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex OrderIdRegex();

    [GeneratedRegex(@"(\d{4})\s+([\w-]+)\s+([\w\s-]+?)(?:\s+(?:SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon))",
        RegexOptions.IgnoreCase)]
    private static partial Regex VehicleInfoRegex();

    [GeneratedRegex(@"([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"\b(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    // Contact name pattern: extracts name after "Name:" until we hit a street number or state+zip
    [GeneratedRegex(@"Name:\s*(.+?)(?=\d{2,}|\b[A-Z]{2}\s+\d{5})", RegexOptions.IgnoreCase)]
    private static partial Regex ContactNameRegex();

    // Address pattern for concatenated text: captures street (starting with number), city, state, zip
    // Example: "1190 Lancaster RdManheim, PA 17545-9746" or "3877 Walnut hill lane,Dallas, TX 75229"
    [GeneratedRegex(
        @"(\d+[^,]*?(?:Rd|Road|St|Street|Ave|Avenue|Blvd|Boulevard|Dr|Drive|Ln|Lane|Way|Ct|Court|Pl|Place|Cir|Circle)[^,]*?),?\s*([A-Za-z\s]+),\s*([A-Z]{2})\s+(\d{5}(?:-\d{4})?)",
        RegexOptions.IgnoreCase)]
    private static partial Regex ConcatenatedAddressRegex();

    [GeneratedRegex(@"Carrier Pickup Not Later Than\*?:\s*(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"Carrier Delivery Not Later Than\*?:\s*(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    [GeneratedRegex(@"Total Payment to Carrier:\s*\$?([\d,]+\.?\d*)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    [GeneratedRegex(@"^(.+?)Shipper", RegexOptions.Singleline)]
    private static partial Regex ShipperNameRegex();
}
