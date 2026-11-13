using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Terminal : AuditableEntity, ITenantEntity
{
    public required string Name { get; set; }

    private string _code = "";

    /// <summary>Terminal code, stored trimmed and upper-case by <see cref="NormalizeCode"/>.</summary>
    public required string Code
    {
        get => _code;
        set => _code = NormalizeCode(value);
    }

    /// <summary>Trims and upper-cases a terminal code. Queries by code apply it to the search term, since stored codes are normalized.</summary>
    public static string NormalizeCode(string? value) =>
        value?.Trim().ToUpperInvariant() ?? "";

    public required TerminalType Type { get; set; }

    public required Address Address { get; set; }

    public string? Notes { get; set; }
}
