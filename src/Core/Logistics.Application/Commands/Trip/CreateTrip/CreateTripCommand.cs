using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateTripCommand : IAppRequest
{
    public string Name { get; set; } = null!;
    public DateTime PlannedStart { get; set; }
    public Guid TruckId { get; set; }
    public IEnumerable<CreateTripLoadCommand>? NewLoads { get; set; }
    public IEnumerable<Guid>? AttachLoadIds { get; set; }
}
