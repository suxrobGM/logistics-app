using Logistics.Domain.UserAggregate;

namespace Logistics.Domain.TruckAggregate;

public class Truck : IAggregateRoot
{
    public string Id { get; set; } = Generator.NewGuid();
    public int? TruckNumber { get; set; }
    
    public User Driver { get; set; }
    public string DriverId { get; set; }
}