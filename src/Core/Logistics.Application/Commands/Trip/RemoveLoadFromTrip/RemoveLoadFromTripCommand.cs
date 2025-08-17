using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RemoveLoadFromTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
    public Guid LoadId { get; set; }
}
