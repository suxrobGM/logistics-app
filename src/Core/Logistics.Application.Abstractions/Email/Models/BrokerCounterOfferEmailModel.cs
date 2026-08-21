namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
/// Model for the broker-facing counter-offer email template.
/// </summary>
public record BrokerCounterOfferEmailModel
{
    public string? BrokerName { get; init; }
    public required string OriginCity { get; init; }
    public required string OriginState { get; init; }
    public required string DestinationCity { get; init; }
    public required string DestinationState { get; init; }
    public required string PickupDate { get; init; }
    public required string EquipmentType { get; init; }
    public required string OfferTotal { get; init; }
    public string? OfferPerMile { get; init; }
    public required string Message { get; init; }
    public required string CompanyName { get; init; }
    public string? CompanyMcNumber { get; init; }
    public required string ReferenceNumber { get; init; }
    public required string ReplyInstructions { get; init; }
}
