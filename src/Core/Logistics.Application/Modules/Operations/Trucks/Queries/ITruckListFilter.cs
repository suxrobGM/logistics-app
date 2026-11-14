using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

/// <summary>The filters the truck list and its summary share, so the totals always describe the list.</summary>
public interface ITruckListFilter
{
    string? Search { get; }
    TruckStatus[]? Statuses { get; }
    TruckType[]? Types { get; }
    bool Idle { get; }
}
