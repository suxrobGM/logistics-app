using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
    public string? Name { get; set; }
    public DateTime? PlannedStart { get; set; }
    public IEnumerable<Guid>? Loads { get; set; }
}
