using Logistics.Domain.Shared;

namespace Logistics.Application.Contracts.Filters;

public class FilterCargo
{
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal MinPricePerMile { get; set; } = (decimal)CargoConsts.MinPricePerMile;
    public decimal MaxPricePerMile { get; set; } = (decimal)CargoConsts.MaxPricePerMile;
    public double MinTripMiles { get; set; } = CargoConsts.MinTripMiles;
    public double MaxTripMiles { get; set; } = CargoConsts.MaxTripMiles;
    public string? AssignedTruckDriverName { get; set; }
    public string? Status { get; set; }
    public bool IsCompleted { get; set; }
}
