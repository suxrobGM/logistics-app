using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an intermodal terminal - sea port, rail terminal, inland depot,
/// air cargo facility, or border crossing - used as a pickup / drop-off point.
/// </summary>
public class Terminal : AuditableEntity, ITenantEntity
{
    public required string Name { get; set; }

    private string code = "";

    /// <summary>
    /// UN/LOCODE identifier (e.g. "BEANR" Antwerp, "USLAX" Los Angeles, "DEHAM" Hamburg).
    /// <para>
    /// Canonicalised on write by <see cref="NormalizeCode"/> - see <c>Container.Number</c> for why.
    /// </para>
    /// </summary>
    public required string Code
    {
        get => code;
        set => code = NormalizeCode(value);
    }

    /// <summary>
    /// The canonical form of a UN/LOCODE. Callers that query by code must normalise the search term
    /// through this, since the stored value is always canonical.
    /// </summary>
    public static string NormalizeCode(string? value) =>
        value?.Trim().ToUpperInvariant() ?? "";

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g. "BE", "US", "DE").
    /// </summary>
    public required string CountryCode { get; set; }

    public required TerminalType Type { get; set; }

    public required Address Address { get; set; }

    public string? Notes { get; set; }
}
