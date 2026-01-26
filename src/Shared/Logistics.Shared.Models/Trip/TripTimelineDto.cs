namespace Logistics.Shared.Models;

public record TripTimelineDto
{
    public IEnumerable<TripTimelineEventDto> Events { get; set; } = [];
}

public record TripTimelineEventDto
{
    public DateTime Timestamp { get; set; }
    public required string EventType { get; set; }
    public required string Description { get; set; }
    public Guid? StopId { get; set; }
    public Guid? LoadId { get; set; }
    public string? LoadName { get; set; }
}
