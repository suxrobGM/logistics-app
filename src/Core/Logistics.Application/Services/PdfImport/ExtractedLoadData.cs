namespace Logistics.Application.Services.PdfImport;

/// <summary>
/// Data extracted from a dispatch sheet PDF.
/// </summary>
public sealed record ExtractedLoadData
{
    public string? OrderId { get; init; }

    // Vehicle information
    public int? VehicleYear { get; init; }
    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
    public string? VehicleVin { get; init; }
    public string? VehicleType { get; init; }

    // Origin address
    public ExtractedAddress? OriginAddress { get; init; }

    // Destination address
    public ExtractedAddress? DestinationAddress { get; init; }

    // Dates
    public DateTime? PickupDate { get; init; }
    public DateTime? DeliveryDate { get; init; }

    // Payment
    public decimal? PaymentAmount { get; init; }

    // Shipper/Customer info
    public string? ShipperName { get; init; }
    public string? ShipperPhone { get; init; }
    public string? ShipperEmail { get; init; }

    /// <summary>
    /// The template that was used to extract this data.
    /// </summary>
    public string? SourceTemplate { get; init; }

    /// <summary>
    /// Gets the vehicle description for use as Load name.
    /// </summary>
    public string GetLoadName()
    {
        if (VehicleYear.HasValue && !string.IsNullOrEmpty(VehicleMake) && !string.IsNullOrEmpty(VehicleModel))
        {
            return $"{VehicleYear} {VehicleMake} {VehicleModel}";
        }

        return OrderId ?? "Imported Load";
    }
}

/// <summary>
/// Address extracted from PDF.
/// </summary>
public sealed record ExtractedAddress
{
    public string? Line1 { get; init; }
    public string? Line2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string Country { get; init; } = "USA";

    /// <summary>
    /// Contact name at this address.
    /// </summary>
    public string? ContactName { get; init; }

    /// <summary>
    /// Phone number for this location.
    /// </summary>
    public string? Phone { get; init; }
}
