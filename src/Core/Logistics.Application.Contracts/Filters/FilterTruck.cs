namespace Logistics.Application.Contracts.Filters;

public class FilterTruck : BaseFilter
{
    public int? TruckNumber { get; set; }
    public string? DriverName { get; set; }
}
