using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class Load
{
    /// <summary>
    /// Vehicles carried by a vehicle (car hauler) load; empty for every other load type.
    /// Set through <see cref="SetVehicles"/>.
    /// </summary>
    public List<LoadVehicle> Vehicles { get; private set; } = [];

    public LoadVehicle? FindVehicle(Guid vehicleId) => Vehicles.FirstOrDefault(v => v.Id == vehicleId);

    /// <summary>
    /// Replaces the vehicles on the load. Set <see cref="Type"/> first: only a vehicle load can list vehicles.
    /// </summary>
    /// <exception cref="InvalidOperationException">Vehicles given for a load that is not a vehicle load.</exception>
    public void SetVehicles(IEnumerable<LoadVehicle> vehicles)
    {
        List<LoadVehicle> list = [.. vehicles];
        if (Type != LoadType.Vehicle && list.Count > 0)
        {
            throw new InvalidOperationException("Only vehicle loads can list vehicles.");
        }

        Vehicles = list;
    }
}
