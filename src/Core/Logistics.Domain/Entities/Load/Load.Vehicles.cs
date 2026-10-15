namespace Logistics.Domain.Entities;

public partial class Load
{
    /// <summary>
    /// Vehicles carried by a vehicle (car hauler) load; empty for every other load type.
    /// </summary>
    public List<LoadVehicle> Vehicles { get; set; } = [];

    public LoadVehicle? FindVehicle(Guid vehicleId) => Vehicles.FirstOrDefault(v => v.Id == vehicleId);
}
