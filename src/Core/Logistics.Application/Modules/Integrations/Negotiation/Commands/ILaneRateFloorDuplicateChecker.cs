namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

/// <summary>
/// (OriginCountry, OriginState, DestinationCountry, DestinationState) uniqueness for lane rate
/// floors, pre-checked ahead of the DB unique index (see LaneRateFloorEntityConfiguration) so the
/// handler returns a friendly message instead of surfacing a raw constraint violation.
/// </summary>
public interface ILaneRateFloorDuplicateChecker : IApplicationService
{
    /// <summary>
    /// Returns an error message when another lane rate floor already covers the same lane, or
    /// null when the write is safe. Pass <paramref name="excludeId"/> when updating an existing
    /// row so it does not conflict with itself.
    /// </summary>
    Task<string?> FindConflictAsync(
        string originCountry,
        string? originState,
        string destinationCountry,
        string? destinationState,
        Guid? excludeId,
        CancellationToken ct);
}
