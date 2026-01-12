using System.Globalization;
using System.Text.RegularExpressions;

using Logistics.Application.Services.PdfImport;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
/// Parser for CentralDispatch platform dispatch sheets.
/// </summary>
public sealed partial class CentralDispatchTemplate : IDispatchSheetTemplate
{
    public string TemplateName => "CentralDispatch";

    public bool CanParse(string pdfText)
    {
        return pdfText.Contains("CentralDispatch", StringComparison.OrdinalIgnoreCase)
               || pdfText.Contains("Dispatch Sheet", StringComparison.OrdinalIgnoreCase)
                  && pdfText.Contains("Load ID", StringComparison.OrdinalIgnoreCase);
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

    private static ExtractedAddress? ExtractOriginAddress(string text)
    {
        // Look for Origin section
        var originMatch = OriginSectionRegex().Match(text);
        if (!originMatch.Success) return null;

        var section = originMatch.Value;

        // Extract name (first line after Origin)
        var nameMatch = OriginNameRegex().Match(section);
        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : null;

        // Extract full address
        var addressMatch = OriginAddressRegex().Match(section);
        if (addressMatch.Success)
        {
            var street = addressMatch.Groups[1].Value.Trim();
            var city = addressMatch.Groups[2].Value.Trim();
            var state = addressMatch.Groups[3].Value.Trim();
            var zip = addressMatch.Groups[4].Value.Trim();

            return new ExtractedAddress
            {
                ContactName = name,
                Line1 = street,
                City = city,
                State = state,
                ZipCode = zip
            };
        }

        return null;
    }

    private static ExtractedAddress? ExtractDestinationAddress(string text)
    {
        // Look for Destination section
        var destMatch = DestinationSectionRegex().Match(text);
        if (!destMatch.Success) return null;

        var section = destMatch.Value;

        // Extract name
        var nameMatch = DestNameRegex().Match(section);
        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : null;

        // Extract full address
        var addressMatch = DestAddressRegex().Match(section);
        if (addressMatch.Success)
        {
            var street = addressMatch.Groups[1].Value.Trim();
            var city = addressMatch.Groups[2].Value.Trim();
            var state = addressMatch.Groups[3].Value.Trim();
            var zip = addressMatch.Groups[4].Value.Trim();

            return new ExtractedAddress
            {
                ContactName = name,
                Line1 = street,
                City = city,
                State = state,
                ZipCode = zip
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
        var match = ShipperNameRegex().Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // Regex patterns
    [GeneratedRegex(@"Load ID\s+(.+?)(?:\n|Total)", RegexOptions.IgnoreCase)]
    private static partial Regex LoadIdRegex();

    [GeneratedRegex(@"Vehicle Year/Make/Model\s+(\d{4})\s+([\w]+)\s+([\w]+)", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleYearMakeModelRegex();

    [GeneratedRegex(@"VIN\s+([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"Vehicle Type\s+(SUV|Sedan|Truck|Van|Coupe|Convertible|Hatchback|Wagon)", RegexOptions.IgnoreCase)]
    private static partial Regex VehicleTypeRegex();

    [GeneratedRegex(@"Origin Info.*?(?=Destination Info|Load Info|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex OriginSectionRegex();

    [GeneratedRegex(@"Destination Info.*?(?=Dates|Load Info|Vehicle Info|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex DestinationSectionRegex();

    [GeneratedRegex(@"Origin\s+([^\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex OriginNameRegex();

    [GeneratedRegex(@"Destination\s+([^\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DestNameRegex();

    [GeneratedRegex(@"(\d+[^\n,]+)\n([^,\n]+),\s*([A-Z]{2})\s*(\d{5})", RegexOptions.IgnoreCase)]
    private static partial Regex OriginAddressRegex();

    [GeneratedRegex(@"(\d+[^\n,]+)\n([^,\n]+),\s*([A-Z]{2})\s*(\d{5})", RegexOptions.IgnoreCase)]
    private static partial Regex DestAddressRegex();

    [GeneratedRegex(@"Scheduled Pick-Up\s+(?:Exactly\s+)?(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PickupDateRegex();

    [GeneratedRegex(@"Scheduled Delivery\s+(?:Exactly\s+)?(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryDateRegex();

    [GeneratedRegex(@"Total Price\s*\$?([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentRegex();

    [GeneratedRegex(@"Shipper\s+([^\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ShipperNameRegex();
}
