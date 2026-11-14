using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>Status counts and revenue over every load the list filters match, not one page.</summary>
public class GetLoadsSummaryQuery : IQuery<Result<LoadsSummaryDto>>, ILoadListFilter
{
    public string? Search { get; set; }
    public bool ActiveOnly { get; set; }
    public bool Unassigned { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public LoadStatus[]? Statuses { get; set; }
    public LoadType[]? Types { get; set; }
    public Guid? CustomerId { get; set; }
}
