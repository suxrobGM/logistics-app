using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelTripCommand : ICommand
{
    public Guid TripId { get; set; }
    public string? Reason { get; set; }
}
