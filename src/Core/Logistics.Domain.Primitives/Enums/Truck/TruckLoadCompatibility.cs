namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// The hard truck/load type pairings: car haulers carry only vehicles and container trucks only
/// containers, in both directions. Every other pairing is allowed here - whether a reefer load fits a
/// tractor depends on the trailer it pulls, which is a dispatcher's call, not a rule.
/// </summary>
public static class TruckLoadCompatibility
{
    public static IReadOnlyList<TruckType> VehicleTruckTypes { get; } = [TruckType.CarHauler, TruckType.CarTransporter];
    public static IReadOnlyList<LoadType> VehicleLoadTypes { get; } = [LoadType.Vehicle];

    public static IReadOnlyList<TruckType> ContainerTruckTypes { get; } = [TruckType.ContainerTruck];
    public static IReadOnlyList<LoadType> ContainerLoadTypes { get; } =
        [LoadType.IntermodalContainer, LoadType.TankContainer, LoadType.ReeferContainer];

    public static bool CanHaul(TruckType truckType, LoadType loadType) =>
        VehicleTruckTypes.Contains(truckType) == VehicleLoadTypes.Contains(loadType)
        && ContainerTruckTypes.Contains(truckType) == ContainerLoadTypes.Contains(loadType);

    /// <summary>"Truck T-1 is a CarHauler and can only carry Vehicle loads." Null when the pairing is allowed.</summary>
    public static string? DescribeMismatch(string truckNumber, TruckType truckType, LoadType loadType)
    {
        if (CanHaul(truckType, loadType))
            return null;

        if (VehicleTruckTypes.Contains(truckType))
            return $"Truck {truckNumber} is a {truckType} and can only carry {Join(VehicleLoadTypes)} loads.";

        if (ContainerTruckTypes.Contains(truckType))
            return $"Truck {truckNumber} is a {truckType} and can only carry {Join(ContainerLoadTypes)} loads.";

        var required = VehicleLoadTypes.Contains(loadType) ? VehicleTruckTypes : ContainerTruckTypes;
        return $"A {loadType} load needs a {string.Join(" or ", required)}; truck {truckNumber} is a {truckType}.";
    }

    private static string Join<T>(IEnumerable<T> values) => string.Join(", ", values);
}
