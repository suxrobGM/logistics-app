using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTripCommand : IRequest<Result>
{
    public Guid TripId { get; set; }
    public string? Name { get; set; }
    public DateTime? PlannedStart { get; set; }
    public IEnumerable<Guid>? Loads { get; set; }
}
