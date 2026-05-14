using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DispatchTripCommand : ICommand
{
    public Guid TripId { get; set; }
}
