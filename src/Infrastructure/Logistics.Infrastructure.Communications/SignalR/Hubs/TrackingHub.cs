using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams and records tenant-scoped truck geolocation.</summary>
public class TrackingHub(
    ITruckGeolocationUpdater geolocationUpdater,
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

    /// <summary>Subscribe to updates for a specific trip.</summary>
    public Task SubscribeToTrip(string tripId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"{TripGroupPrefix}{tripId}");
    }

    /// <summary>Unsubscribe from updates for a specific trip.</summary>
    public Task UnsubscribeFromTrip(string tripId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{TripGroupPrefix}{tripId}");
    }
}
