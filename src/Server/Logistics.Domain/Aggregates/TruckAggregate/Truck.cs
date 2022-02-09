using Logistics.Domain.UserAggregate;

namespace Logistics.Domain.TruckAggregate;

public class Truck : Entity
{
    public int? TruckNumber { get; set; }
    
    public User Driver { get; set; }
    public string DriverId { get; set; }
}