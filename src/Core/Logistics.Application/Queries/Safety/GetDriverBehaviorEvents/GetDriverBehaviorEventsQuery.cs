using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverBehaviorEventsQuery : SearchableQuery, IAppRequest<PagedResult<DriverBehaviorEventDto>>
{
    public Guid? DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public DriverBehaviorEventType? EventType { get; set; }
    public bool? IsReviewed { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
