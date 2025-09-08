namespace Logistics.Shared.Models;

public record OptimizedTripStopsDto
{
    public double TotalDistance { get; set; }
    public IEnumerable<TripStopDto> OrderedStops { get; set; } = null!;
}
