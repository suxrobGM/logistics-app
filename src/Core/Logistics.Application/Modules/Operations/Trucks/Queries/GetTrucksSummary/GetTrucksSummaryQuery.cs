using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

/// <summary>Status counts over every truck the list filters match, not one page.</summary>
public class GetTrucksSummaryQuery : IQuery<Result<TrucksSummaryDto>>, ITruckListFilter
{
    public string? Search { get; set; }
    public TruckStatus[]? Statuses { get; set; }
    public TruckType[]? Types { get; set; }
    public bool Idle { get; set; }
}
