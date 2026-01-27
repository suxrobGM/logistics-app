namespace Logistics.Application.Contracts.Models.Email;

/// <summary>
///     Model for tracking link email templates.
/// </summary>
public record TrackingLinkEmailModel
{
    public required string CompanyName { get; init; }
    public required long LoadNumber { get; init; }
    public string? LoadName { get; init; }
    public required string OriginCity { get; init; }
    public required string DestinationCity { get; init; }
    public string? PersonalMessage { get; init; }
    public required string TrackingUrl { get; init; }
    public required string ExpiresAt { get; init; }
}
