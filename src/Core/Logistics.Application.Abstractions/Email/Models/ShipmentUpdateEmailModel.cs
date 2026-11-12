namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
///     Model shared by the shipment update emails (dispatched, picked up, delivered, late).
///     Times are already formatted in the carrier's time zone.
/// </summary>
public record ShipmentUpdateEmailModel
{
    public required string CompanyName { get; init; }
    public required long LoadNumber { get; init; }
    public string? LoadName { get; init; }
    public required string OriginCity { get; init; }
    public required string DestinationCity { get; init; }

    /// <summary>
    ///     The appointment window the email is about: pickup before pickup, delivery after.
    /// </summary>
    public string? Window { get; init; }

    public string? Eta { get; init; }
    public string? TrackingUrl { get; init; }
    public required string ShipmentUrl { get; init; }

    /// <summary>
    ///     The dispatcher-approved delay notice, on the late email only.
    /// </summary>
    public string? Message { get; init; }

    public string? DeliveredAt { get; init; }
    public string? ReceiverName { get; init; }

    /// <summary>
    ///     One line per vehicle on a vehicle load; empty otherwise. Turns the delivered email into the eBOL notice.
    /// </summary>
    public List<string> Vehicles { get; init; } = [];
}
