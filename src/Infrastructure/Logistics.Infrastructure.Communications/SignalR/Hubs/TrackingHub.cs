using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams and records tenant-scoped truck geolocation.</summary>
public class TrackingHub(
    ITruckGeolocationUpdater geolocationUpdater,
    ITripAccess tripAccess,
    TrackingHubContext hubContext) : TenantHub<ITrackingHubClient>
{
    private const string TripGroupPrefix = "trip:";

    protected override Task OnTenantConnectedAsync(Guid tenantId, Guid userId)
    {
        hubContext.AddClient(Context.ConnectionId, null);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var geolocationData = hubContext.GetGeolocationData(Context.ConnectionId);

        if (geolocationData != null)
        {
            await geolocationUpdater.UpdateAsync(geolocationData);
        }

        hubContext.RemoveClient(Context.ConnectionId);
    }

    /// <summary>Records a position report for a truck assigned to the caller.</summary>
    [Authorize(Roles = TenantRoles.Driver)]
    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } driverId)
        {
            return;
        }

        if (!await geolocationUpdater.CanDriverReportForTruckAsync(
                tenantId, truckGeolocation.TruckId, driverId))
        {
            return;
        }

        truckGeolocation.TenantId = tenantId;

        await Clients
            .Group(tenantId.ToString())
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }

    /// <summary>Subscribe to a trip the caller dispatches or drives.</summary>
    public async Task SubscribeToTrip(string tripId)
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } userId ||
            !Guid.TryParse(tripId, out var tripGuid))
        {
            return;
        }

        if (!await tripAccess.CanUserViewTripAsync(tenantId, tripGuid, userId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TripGroup(tripGuid));
    }

    /// <summary>Unsubscribe from updates for a specific trip.</summary>
    public Task UnsubscribeFromTrip(string tripId)
    {
        return Guid.TryParse(tripId, out var tripGuid)
            ? Groups.RemoveFromGroupAsync(Context.ConnectionId, TripGroup(tripGuid))
            : Task.CompletedTask;
    }

    // Built from the Guid, not the caller's raw string: the broadcast side formats the group the
    // same way, so a braced or upper-case id would join a group nothing ever targets.
    private static string TripGroup(Guid tripId) => $"{TripGroupPrefix}{tripId}";
}
