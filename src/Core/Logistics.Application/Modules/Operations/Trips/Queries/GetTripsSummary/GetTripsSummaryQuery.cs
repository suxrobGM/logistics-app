using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

/// <summary>Status counts over every trip the list filters match, not one page.</summary>
public sealed class GetTripsSummaryQuery : IQuery<Result<TripsSummaryDto>>, ITripListFilter
{
    public string? Search { get; set; }
    public TripStatus[]? Statuses { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool ActiveOnly { get; set; }
}
