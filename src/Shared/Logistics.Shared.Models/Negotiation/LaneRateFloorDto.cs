using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record LaneRateFloorDto
{
    public Guid Id { get; set; }
    public string OriginCountry { get; set; } = "US";
    public string? OriginState { get; set; }
    public string DestinationCountry { get; set; } = "US";
    public string? DestinationState { get; set; }
    public decimal MinRatePerMile { get; set; }
    public Money? MinTotalRate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
