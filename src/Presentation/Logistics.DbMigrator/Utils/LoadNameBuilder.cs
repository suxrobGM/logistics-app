using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Regions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Utils;

/// <summary>
/// Builds realistic, type-aware load names for seeded data so demo dashboards
/// look meaningful instead of "Freight Load 1".."Freight Load 100".
/// </summary>
internal static class LoadNameBuilder
{
    /// <summary>
    /// Builds a name appropriate for the load type. For container types, terminal codes
    /// are preferred over city names if origin/destination terminals are provided.
    /// </summary>
    public static string Build(
        LoadType type,
        Customer customer,
        Address origin,
        Address destination,
        Container? container,
        Terminal? originTerminal,
        Terminal? destinationTerminal,
        IRegionProfile region,
        Random random,
        int? vehicleCount = null)
    {
        return type switch
        {
            LoadType.RefrigeratedGoods or LoadType.ReeferContainer =>
                $"Reefer: {random.Pick((IList<string>)region.ReeferCargoTypes)} - {origin.City} → {destination.City}",

            LoadType.HazardousMaterials =>
                $"HazMat: {random.Pick((IList<string>)region.HazMatCargoTypes)} - {origin.City} → {destination.City}",

            LoadType.IntermodalContainer when container is not null =>
                $"Container {container.Number} - {CodeOrCity(originTerminal, origin)} → {CodeOrCity(destinationTerminal, destination)}",

            LoadType.TankContainer =>
                $"Tank: {random.Pick((IList<string>)region.TankCargoTypes)} - {CodeOrCity(originTerminal, origin)} → {CodeOrCity(destinationTerminal, destination)}",

            LoadType.Vehicle =>
                BuildVehicleName(region, random, origin, destination, vehicleCount),

            _ => $"{customer.Name} - {origin.City} → {destination.City}"
        };
    }

    private static string BuildVehicleName(
        IRegionProfile region,
        Random random,
        Address origin,
        Address destination,
        int? vehicleCount)
    {
        var picked = random.Pick((IList<VehicleMakeModel>)region.CarHaulerCargoVehicles);
        var count = vehicleCount ?? random.Next(2, 8);
        return $"{count}x {picked.Make} {picked.Model} - {origin.City} → {destination.City}";
    }

    private static string CodeOrCity(Terminal? terminal, Address fallback)
    {
        return terminal?.Code ?? fallback.City;
    }
}
