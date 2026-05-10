using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Driver license endorsements. Stored as a flags column so a single license can carry
/// multiple endorsements (e.g., US CDL with Hazmat + Tanker, or EU CE with ADR + ADR Class 7).
/// </summary>
[Flags]
public enum LicenseEndorsement
{
    None = 0,

    // US CDL endorsements
    [Description("Hazmat (H)")]
    Hazmat = 1 << 0,

    [Description("Tanker (N)")]
    Tanker = 1 << 1,

    [Description("Doubles/Triples (T)")]
    Doubles = 1 << 2,

    [Description("Passenger (P)")]
    Passenger = 1 << 3,

    // EU / ADR endorsements
    [Description("ADR (Basic)")]
    Adr = 1 << 4,

    [Description("ADR Tanks")]
    AdrTanks = 1 << 5,

    [Description("ADR Class 1 (Explosives)")]
    AdrClass1 = 1 << 6,

    [Description("ADR Class 7 (Radioactive)")]
    AdrClass7 = 1 << 7
}

public static class LicenseEndorsementExtensions
{
    private static readonly LicenseEndorsement[] AllValues =
    [
        LicenseEndorsement.Hazmat,
        LicenseEndorsement.Tanker,
        LicenseEndorsement.Doubles,
        LicenseEndorsement.Passenger,
        LicenseEndorsement.Adr,
        LicenseEndorsement.AdrTanks,
        LicenseEndorsement.AdrClass1,
        LicenseEndorsement.AdrClass7
    ];

    /// <summary>
    /// Expands a flags value into the set of individual endorsements it contains.
    /// </summary>
    public static LicenseEndorsement[] ToArray(this LicenseEndorsement value) =>
        AllValues.Where(v => value.HasFlag(v)).ToArray();

    /// <summary>
    /// Combines a set of endorsements into a single flags bitfield.
    /// </summary>
    public static LicenseEndorsement Combine(this IEnumerable<LicenseEndorsement>? values) =>
        values?.Aggregate(LicenseEndorsement.None, (acc, v) => acc | v) ?? LicenseEndorsement.None;
}
