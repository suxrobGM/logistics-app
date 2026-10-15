namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
///     Model for the car hauler delivery email that points the customer at the electronic BOL.
/// </summary>
public record VehicleDeliveredEmailModel
{
    public required string CompanyName { get; init; }
    public required long LoadNumber { get; init; }
    public string? LoadName { get; init; }
    public required string DestinationCity { get; init; }
    public required string DeliveredAt { get; init; }
    public string? ReceiverName { get; init; }
    public required List<string> Vehicles { get; init; }
    public required string ShipmentUrl { get; init; }
}
