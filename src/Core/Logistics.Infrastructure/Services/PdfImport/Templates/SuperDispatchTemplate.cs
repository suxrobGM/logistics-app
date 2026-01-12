using System.Globalization;
using System.Text.RegularExpressions;

using Logistics.Application.Services.PdfImport;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
/// Parser for Super Dispatch platform dispatch sheets.
/// </summary>
public sealed partial class SuperDispatchTemplate : IDispatchSheetTemplate
{
    public string TemplateName => "Super Dispatch";

    public bool CanParse(string pdfText)
    {
        return pdfText.Contains("Powered by Super Dispatch", StringComparison.OrdinalIgnoreCase)
               || pdfText.Contains("Super Dispatch", StringComparison.OrdinalIgnoreCase);
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

    private static ExtractedAddress? ExtractOriginAddress(string text)
    {
        // Look for Pickup Information section
        var pickupMatch = PickupSectionRegex().Match(text);
        if (!pickupMatch.Success) return null;

        var section = pickupMatch.Value;

        // Extract name and address from section
        var nameMatch = PickupNameRegex().Match(section);
        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : null;

        // Extract address components
        var addressMatch = AddressRegex().Match(section);
        if (addressMatch.Success)
        {
            return new ExtractedAddress
            {
                ContactName = name,
                Line1 = addressMatch.Groups[1].Value.Trim(),
                City = addressMatch.Groups[2].Value.Trim(),
                State = addressMatch.Groups[3].Value.Trim(),
                ZipCode = addressMatch.Groups[4].Value.Trim()
            };
        }

        return null;
    }

    private static ExtractedAddress? ExtractDestinationAddress(string text)
    {
        // Look for Delivery Information section
        var deliveryMatch = DeliverySectionRegex().Match(text);
        if (!deliveryMatch.Success) return null;

        var section = deliveryMatch.Value;

        // Extract name
        var nameMatch = DeliveryNameRegex().Match(section);
        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : null;

        // Extract address components
        var addressMatch = AddressRegex().Match(section);
        if (addressMatch.Success)
        {
            return new ExtractedAddress
            {
                ContactName = name,
                Line1 = addressMatch.Groups[1].Value.Trim(),
                City = addressMatch.Groups[2].Value.Trim(),
                State = addressMatch.Groups[3].Value.Trim(),
                ZipCode = addressMatch.Groups[4].Value.Trim()
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
        // Look for shipper name at the beginning
        var match = ShipperNameRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // Regex patterns
    [GeneratedRegex(@"Order ID:\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex OrderIdRegex();

    [GeneratedRegex(@"(\d{4})\s+([\w-]+)\s+([\w\s-]+?)(?:\s+(?:SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon))", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleInfoRegex();

    [GeneratedRegex(@"([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"\b(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    [GeneratedRegex(@"Pickup Information.*?(?=Delivery Information|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex PickupSectionRegex();

    [GeneratedRegex(@"Delivery Information.*?(?=\*|REVISIONS|CONTRACT|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex DeliverySectionRegex();

    [GeneratedRegex(@"Name:\s*(.+?)(?:\n|Phone)", RegexOptions.IgnoreCase)]
    private static partial Regex PickupNameRegex();

    [GeneratedRegex(@"Name:\s*(.+?)(?:\n|Phone)", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryNameRegex();

    [GeneratedRegex(@"(\d+[^,\n]+),\s*([^,]+),\s*([A-Z]{2})\s*(\d{5}(?:-\d{4})?)", RegexOptions.IgnoreCase)]
    private static partial Regex AddressRegex();

    [GeneratedRegex(@"(?:Carrier Pickup Not Later Than|Pickup.*?Exactly)[:\s]*(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"(?:Carrier Delivery Not Later Than|Delivery.*?Exactly)[:\s]*(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    [GeneratedRegex(@"Total Payment to Carrier:\s*\$?([\d,]+\.?\d*)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    [GeneratedRegex(@"^([^\n]+?)(?:\s+Shipper|\s+USDOT|\n)", RegexOptions.Multiline)]
    private static partial Regex ShipperNameRegex();
}
