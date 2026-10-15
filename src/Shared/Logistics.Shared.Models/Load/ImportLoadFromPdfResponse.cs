using Logistics.Domain.Primitives.ValueObjects;

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

    /// <summary>
    /// Every vehicle listed on the dispatch sheet.
    /// </summary>
    public List<LoadVehicleInput> Vehicles { get; init; } = [];

    public Address? OriginAddress { get; init; }
    public Address? DestinationAddress { get; init; }
    public string? OriginContactName { get; init; }
    public string? OriginContactPhone { get; init; }
    public string? DestinationContactName { get; init; }
    public string? DestinationContactPhone { get; init; }
    public DateTime? PickupDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
    public decimal? PaymentAmount { get; init; }
    public string? ShipperName { get; init; }
    public string? ShipperPhone { get; init; }
    public string? ShipperEmail { get; init; }
    public string? SourceTemplate { get; init; }

    /// <summary>
    /// The load name: the single vehicle's description, a count for several, or the order id.
    /// </summary>
    public string GetLoadName()
    {
        if (Vehicles.Count > 1)
        {
            return $"{Vehicles.Count} vehicles{(OrderId is null ? "" : $" - {OrderId}")}";
        }

        if (Vehicles is [{ Year: not null, Make: not null, Model: not null } vehicle])
        {
            return $"{vehicle.Year} {vehicle.Make} {vehicle.Model}";
        }

        return OrderId ?? "Imported Load";
    }
}
