using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Services.Realtime;

/// <summary>
///     Abstraction for real-time live tracking (GPS, trip updates).
/// </summary>
public interface IRealtimeLiveTrackingService
{
    Task BroadcastGeolocationDataAsync(
        string tenantId,
        TruckGeolocationDto geolocationData,
        CancellationToken cancellationToken = default);

    Task BroadcastTripStatusUpdateAsync(
        string tenantId,
        Guid tripId,
        TripStatus newStatus,
        CancellationToken cancellationToken = default);

    Task BroadcastStopArrivalAsync(
        string tenantId,
        Guid tripId,
        Guid stopId,
        CancellationToken cancellationToken = default);

    Task BroadcastDispatchBoardUpdateAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}
