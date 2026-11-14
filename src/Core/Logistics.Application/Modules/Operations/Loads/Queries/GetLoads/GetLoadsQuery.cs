using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

public class GetLoadsQuery : SearchableQuery, IQuery<PagedResult<LoadDto>>
{
    public bool ActiveOnly { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public LoadStatus[]? Statuses { get; set; }
    public LoadType[]? Types { get; set; }
    public Guid? CustomerId { get; set; }
}
