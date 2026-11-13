namespace Logistics.Application.Abstractions.Communications.EmailModels;

/// <summary>Times are already formatted in the carrier's time zone.</summary>
public record ShipmentUpdateEmailModel
{
    public required string CompanyName { get; init; }
    public required long LoadNumber { get; init; }
    public string? LoadName { get; init; }
    public required string OriginCity { get; init; }
    public required string DestinationCity { get; init; }

    /// <summary>The pickup window before pickup, the delivery window after.</summary>
    public string? Window { get; init; }

    public string? Eta { get; init; }
    public string? TrackingUrl { get; init; }
    public required string ShipmentUrl { get; init; }

    /// <summary>Late email only.</summary>
    public string? Message { get; init; }

    public string? DeliveredAt { get; init; }
    public string? ReceiverName { get; init; }

    /// <summary>Non-empty only on a vehicle load, where it turns the delivered email into the eBOL notice.</summary>
    public List<string> Vehicles { get; init; } = [];
}
