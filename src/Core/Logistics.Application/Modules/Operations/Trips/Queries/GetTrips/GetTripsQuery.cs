using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

public sealed class GetTripsQuery : PagedQuery, IQuery<PagedResult<TripDto>>
{
    public string? Name { get; set; }
    public TripStatus? Status { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool OnlyActiveTrips { get; set; }
}
