using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record FuelCardSyncResultDto
{
    /// <summary>New transactions imported this run (excludes already-known ones).</summary>
    public int Imported { get; set; }

    /// <summary>Imported transactions auto-matched to a truck and materialized as expenses.</summary>
    public int Matched { get; set; }

    /// <summary>Imported transactions left in the review queue.</summary>
    public int Pending { get; set; }

    public Dictionary<FuelCardProviderType, string?>? Errors { get; set; }
}
