using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
/// The one map between <see cref="LoadBoardEquipmentType"/> and DAT's equipment codes, from DAT's
/// published list (one.support.dat.com, "Equipment Types"). DAT has no combined codes such as
/// van-or-reefer except <c>FD</c> (flatbed or step deck).
/// </summary>
internal static class DatEquipment
{
    /// <summary>
    /// The codes a search sends for each type: the base code, plus the siblings DAT splits a
    /// type into where no base code exists (tankers, containers).
    /// </summary>
    private static readonly Dictionary<LoadBoardEquipmentType, string[]> SearchCodes = new()
    {
        [LoadBoardEquipmentType.Van] = ["V"],
        [LoadBoardEquipmentType.Reefer] = ["R"],
        [LoadBoardEquipmentType.Flatbed] = ["F"],
        [LoadBoardEquipmentType.StepDeck] = ["SD"],
        [LoadBoardEquipmentType.DoubleDrop] = ["DD"],
        [LoadBoardEquipmentType.Lowboy] = ["LB"],
        [LoadBoardEquipmentType.RemovableGooseneck] = ["RG"],
        [LoadBoardEquipmentType.Conestoga] = ["CN"],
        [LoadBoardEquipmentType.Hotshot] = ["FH"],
        [LoadBoardEquipmentType.PowerOnly] = ["PO"],
        [LoadBoardEquipmentType.AutoCarrier] = ["AC"],
        [LoadBoardEquipmentType.Container] = ["C", "CI", "CR"],
        [LoadBoardEquipmentType.Tanker] = ["TS", "TA", "TN"],
        [LoadBoardEquipmentType.HopperBottom] = ["HB"],
        [LoadBoardEquipmentType.DumpTrailer] = ["DT"],
        [LoadBoardEquipmentType.BoxTruck] = ["SB"],
        [LoadBoardEquipmentType.FlatbedOrStepDeck] = ["FD"]
    };

    /// <summary>
    /// Every DAT code a listing can carry, folded onto the nearest type. Variants (air-ride,
    /// hazmat, team, double...) describe the same trailer to a carrier deciding whether it fits.
    /// </summary>
    private static readonly Dictionary<string, LoadBoardEquipmentType> ByCode = BuildCodeMap();

    public static string ToPostingCode(LoadBoardEquipmentType type) => SearchCodes[type][0];

    public static IEnumerable<string> ToSearchCodes(IEnumerable<LoadBoardEquipmentType> types) =>
        types.SelectMany(t => SearchCodes[t]).Distinct();

    public static LoadBoardEquipmentType? FromCode(string? code) =>
        code is not null && ByCode.TryGetValue(code.Trim().ToUpperInvariant(), out var type) ? type : null;

    /// <summary>
    /// RateView prices only these three trailer families and takes words, not posting codes.
    /// </summary>
    public static string? ToRateViewEquipment(LoadBoardEquipmentType? type) => type switch
    {
        LoadBoardEquipmentType.Van or LoadBoardEquipmentType.BoxTruck => "VAN",
        LoadBoardEquipmentType.Reefer => "REEFER",
        LoadBoardEquipmentType.Flatbed or LoadBoardEquipmentType.StepDeck or LoadBoardEquipmentType.DoubleDrop
            or LoadBoardEquipmentType.Lowboy or LoadBoardEquipmentType.RemovableGooseneck
            or LoadBoardEquipmentType.Conestoga or LoadBoardEquipmentType.Hotshot
            or LoadBoardEquipmentType.FlatbedOrStepDeck => "FLATBED",
        _ => null
    };

    private static Dictionary<string, LoadBoardEquipmentType> BuildCodeMap()
    {
        var map = SearchCodes
            .SelectMany(kv => kv.Value.Select(code => (code, type: kv.Key)))
            .ToDictionary(x => x.code, x => x.type);

        (LoadBoardEquipmentType Type, string[] Codes)[] variants =
        [
            (LoadBoardEquipmentType.Van,
                ["VA", "V2", "VZ", "VH", "VI", "VN", "VG", "OT", "VB", "V3", "VV", "VC", "VM", "VW", "VP", "IR", "MV"]),
            (LoadBoardEquipmentType.Reefer, ["RA", "R2", "RZ", "RN", "RL", "RM", "RP"]),
            (LoadBoardEquipmentType.Flatbed, ["FA", "F2", "FZ", "MX", "FS", "FT", "FM", "FO", "FC", "BT", "ST"]),
            (LoadBoardEquipmentType.Conestoga, ["FN", "SN", "VS"]),
            (LoadBoardEquipmentType.Lowboy, ["LO"]),
            (LoadBoardEquipmentType.StepDeck, ["LA"])
        ];

        foreach (var (type, codes) in variants)
        {
            foreach (var code in codes)
            {
                map[code] = type;
            }
        }

        return map;
    }
}
