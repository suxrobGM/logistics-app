using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

/// <summary>The filters the trip list and its summary share, so the totals always describe the list.</summary>
public interface ITripListFilter
{
    string? Search { get; }
    TripStatus[]? Statuses { get; }
    Guid? TruckId { get; }
    DateTime? FromDate { get; }
    DateTime? ToDate { get; }
    bool ActiveOnly { get; }
}
