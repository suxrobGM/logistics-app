using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class TruckDto
{
    public Guid? Id { get; set; }
    public string? Number { get; set; }
    public TruckType Type { get; set; }
    public TruckStatus Status { get; set; }
    public Address? CurrentAddress { get; set; }
    public GeoPoint? CurrentLocation { get; set; }
    public EmployeeDto? MainDriver { get; set; }
    public EmployeeDto? SecondaryDriver { get; set; }
    public IEnumerable<LoadDto> Loads { get; set; } = [];
}
