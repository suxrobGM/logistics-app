namespace Logistics.Domain.Entities;

public class Truck : Entity
{
    public int? TruckNumber { get; set; }
    public User? Driver { get; set; }
}