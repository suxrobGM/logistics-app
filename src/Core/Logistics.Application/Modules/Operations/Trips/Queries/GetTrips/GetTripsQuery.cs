using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

public sealed class GetTripsQuery : SearchableQuery, IQuery<PagedResult<TripDto>>
{
    public TripStatus? Status { get; set; }
    public TripStatus[]? Statuses { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool ActiveOnly { get; set; }
}
