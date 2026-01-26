using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DispatchTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
}
