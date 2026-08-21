using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Resolves the rate floor that applies to a load board listing's lane, falling back from the
/// most specific configured <see cref="LaneRateFloor"/> row down to the tenant-wide default.
/// </summary>
public interface ILaneRateFloorResolver : IApplicationService
{
    Task<EffectiveRateFloorDto> ResolveAsync(LoadBoardListing listing, CancellationToken ct = default);
}
