using Logistics.Application.Services.Realtime;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRLiveTrackingService : IRealtimeLiveTrackingService
{
    public Task BroadcastGeolocationDataAsync(string tenantId, TruckGeolocationDto geolocationData,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastTripStatusUpdateAsync(string tenantId, Guid tripId, TripStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastStopArrivalAsync(string tenantId, Guid tripId, Guid stopId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastDispatchBoardUpdateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
