using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class MarkStopArrivedCommand : ICommand
{
    public Guid TripId { get; set; }
    public Guid StopId { get; set; }
}
