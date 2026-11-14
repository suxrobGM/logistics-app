using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Services;

public interface ITripLoadFinder : IApplicationService
{
    /// <summary>
    /// Fetches existing loads to put on a trip, failing if any is missing, delivered or cancelled,
    /// or already on a trip other than <paramref name="tripId"/>.
    /// </summary>
    Task<Result<List<Load>>> FindAttachableAsync(IReadOnlyCollection<Guid> loadIds, Guid? tripId, CancellationToken ct);
}
