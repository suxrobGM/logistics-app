using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class UpdateTripCommand : IAppRequest
{
    public Guid TripId { get; set; }
    public string? Name { get; set; }
    public Guid? TruckId { get; set; }

    public IEnumerable<CreateTripLoadCommand>? NewLoads { get; set; }
    public IEnumerable<Guid>? AttachLoadIds { get; set; }
    public IEnumerable<Guid>? DetachLoadIds { get; set; }
}
