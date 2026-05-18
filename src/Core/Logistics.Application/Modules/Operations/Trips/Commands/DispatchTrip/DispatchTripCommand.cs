using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

public class DispatchTripCommand : ICommand
{
    public Guid TripId { get; set; }
}
