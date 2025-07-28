using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class TruckDto
{
    public Guid? Id { get; set; }
    public string? Number { get; set; }
    public TruckType Type { get; set; }
    public TruckStatus Status { get; set; }
    public AddressDto? CurrentLocation { get; set; }
    public double? CurrentLocationLat { get; set; }
    public double? CurrentLocationLong { get; set; }
    public EmployeeDto? MainDriver { get; set; }
    public EmployeeDto? SecondaryDriver { get; set; }
    public IEnumerable<LoadDto> Loads { get; set; } = [];
}
