using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateTripCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
    public DateTime PlannedStart { get; set; }
    public Guid TruckId { get; set; }
    public IEnumerable<Guid>? Loads { get; set; }
}
