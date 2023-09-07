namespace Logistics.Models;

public class TruckDriversDto
{
    public TruckDto? Truck { get; set; }
    public EmployeeDto[]? Drivers { get; set; }
}
