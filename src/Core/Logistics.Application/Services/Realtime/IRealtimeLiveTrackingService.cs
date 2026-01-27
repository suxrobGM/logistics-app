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
        CancellationToken ct = default);

    Task BroadcastTripStatusUpdateAsync(
        string tenantId,
        Guid tripId,
        TripStatus newStatus,
        CancellationToken ct = default);

    Task BroadcastStopArrivalAsync(
        string tenantId,
        Guid tripId,
        Guid stopId,
        CancellationToken ct = default);

    Task BroadcastDispatchBoardUpdateAsync(
        string tenantId,
        CancellationToken ct = default);
}
