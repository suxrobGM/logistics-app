using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>The filters the load list and its summary share, so the totals always describe the list.</summary>
public interface ILoadListFilter
{
    string? Search { get; }
    bool ActiveOnly { get; }
    bool Unassigned { get; }
    Guid? DriverId { get; }
    Guid? TruckId { get; }
    DateTime? FromDate { get; }
    DateTime? ToDate { get; }
    LoadStatus[]? Statuses { get; }
    LoadType[]? Types { get; }
    Guid? CustomerId { get; }
}
