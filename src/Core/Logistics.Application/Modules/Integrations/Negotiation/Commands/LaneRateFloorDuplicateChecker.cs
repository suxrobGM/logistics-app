using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class LaneRateFloorDuplicateChecker(ITenantUnitOfWork tenantUow) : ILaneRateFloorDuplicateChecker
{
    public async Task<string?> FindConflictAsync(
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
