using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
    public string? Reason { get; set; }
}
