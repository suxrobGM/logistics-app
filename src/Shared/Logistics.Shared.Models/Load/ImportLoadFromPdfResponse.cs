namespace Logistics.Shared.Models;

/// <summary>
/// Response from importing a load from a PDF dispatch sheet.
/// </summary>
public record ImportLoadFromPdfResponse
{
    /// <summary>
    /// The ID of the created load.
    /// </summary>
    public Guid LoadId { get; init; }

    /// <summary>
    /// The name/description of the created load.
    /// </summary>
    public string LoadName { get; init; } = string.Empty;

    /// <summary>
    /// The load number assigned to the new load.
    /// </summary>
    public long LoadNumber { get; init; }

    /// <summary>
    /// The data extracted from the PDF.
    /// </summary>
    public ExtractedLoadDataDto? ExtractedData { get; init; }

    /// <summary>
    /// Whether a new customer was created during import.
    /// </summary>
    public bool CustomerCreated { get; init; }

    /// <summary>
    /// The name of the customer (existing or newly created).
    /// </summary>
    public string? CustomerName { get; init; }

    /// <summary>
    /// Any warnings generated during import (non-blocking issues).
    /// </summary>
    public List<string> Warnings { get; init; } = [];
}

/// <summary>
/// DTO for extracted load data from PDF.
/// </summary>
public record ExtractedLoadDataDto
{
    public string? OrderId { get; init; }
    public int? VehicleYear { get; init; }
    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
    public string? VehicleVin { get; init; }
    public string? VehicleType { get; init; }
    public ExtractedAddressDto? OriginAddress { get; init; }
    public ExtractedAddressDto? DestinationAddress { get; init; }
    public DateTime? PickupDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
    public decimal? PaymentAmount { get; init; }
    public string? ShipperName { get; init; }
    public string? SourceTemplate { get; init; }
}

/// <summary>
/// DTO for extracted address from PDF.
/// </summary>
public record ExtractedAddressDto
{
    public string? Line1 { get; init; }
    public string? Line2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public string? ContactName { get; init; }
    public string? Phone { get; init; }
}
