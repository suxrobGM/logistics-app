using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

/// <summary>How well a truck's equipment suits a load, for ranking - never a hard rule.</summary>
internal enum EquipmentMatch
{
    Match,

    /// <summary>A tractor with no trailer on record: it may be fine, nobody can tell.</summary>
    Unconfirmed,

    Mismatch
}

/// <summary>
/// The freight-to-equipment pairings a dispatcher expects. Soft, unlike
/// <see cref="TruckLoadCompatibility"/>: whether a reefer load fits a tractor depends on the
/// trailer it pulls, so these rank candidates rather than exclude them.
/// </summary>
internal static class EquipmentFit
{
    /// <summary>General freight rides in anything except a bare tractor, which needs a box trailer.</summary>
    private static readonly TruckType[] BodyTrucks =
        [.. Enum.GetValues<TruckType>().Where(t => t != TruckType.FreightTruck)];

    public static (EquipmentMatch Match, string? Note) Evaluate(LoadType load, TruckType truck, TrailerType? trailer)
    {
        var (trucks, trailers, need) = Needs(load);

        if (trucks.Contains(truck) || trailer is { } hooked && trailers.Contains(hooked))
            return (EquipmentMatch.Match, null);

        if (truck == TruckType.FreightTruck && trailer is null)
            return (EquipmentMatch.Unconfirmed, $"needs {need}; no trailer on record");

        return (EquipmentMatch.Mismatch, $"needs {need}");
    }

    private static (TruckType[] Trucks, TrailerType[] Trailers, string Need) Needs(LoadType load) => load switch
    {
        LoadType.RefrigeratedGoods or LoadType.Pharmaceutical =>
            ([TruckType.Reefer], [TrailerType.Reefer], "a Reefer truck or trailer"),
        LoadType.Liquid =>
            ([TruckType.Tanker], [TrailerType.Tanker], "a Tanker truck or trailer"),
        LoadType.OversizeHeavy or LoadType.ProjectCargo =>
            ([TruckType.Flatbed, TruckType.LowLoader], [TrailerType.Flatbed, TrailerType.StepDeck, TrailerType.Lowboy],
                "a Flatbed or LowLoader truck, or a Flatbed, StepDeck or Lowboy trailer"),
        LoadType.Bulk => ([TruckType.DumpTruck], [], "a DumpTruck"),
        _ => (BodyTrucks, [TrailerType.DryVan, TrailerType.Conestoga], "a DryVan or Conestoga trailer")
    };
}
