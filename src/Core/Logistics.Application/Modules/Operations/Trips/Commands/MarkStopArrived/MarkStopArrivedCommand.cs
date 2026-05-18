using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

public class MarkStopArrivedCommand : ICommand
{
    public Guid TripId { get; set; }
    public Guid StopId { get; set; }
}
