using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

/// <summary>
/// (OriginCountry, OriginState, DestinationCountry, DestinationState) uniqueness for lane rate
/// floors, pre-checked ahead of the DB unique index (see LaneRateFloorEntityConfiguration) so the
/// handler returns a friendly message instead of surfacing a raw constraint violation.
/// </summary>
internal static class LaneRateFloorUniqueness
{
    /// <summary>
    /// Returns an error message when another lane rate floor already covers the same lane, or
    /// null when the write is safe. Pass <paramref name="excludeId"/> when updating an existing
    /// row so it does not conflict with itself.
    /// </summary>
    public static async Task<string?> FindConflictAsync(
        ITenantUnitOfWork tenantUow,
        string originCountry,
        string? originState,
        string destinationCountry,
        string? destinationState,
        Guid? excludeId,
        CancellationToken ct)
    {
        var duplicate = await tenantUow.Repository<LaneRateFloor>().GetAsync(
            f => (excludeId == null || f.Id != excludeId) &&
                 f.OriginCountry == originCountry && f.OriginState == originState &&
                 f.DestinationCountry == destinationCountry && f.DestinationState == destinationState, ct);

        if (duplicate is null)
        {
            return null;
        }

        var origin = originState is null ? originCountry : $"{originState}, {originCountry}";
        var destination = destinationState is null ? destinationCountry : $"{destinationState}, {destinationCountry}";
        return $"A rate floor for {origin} -> {destination} already exists";
    }
}
