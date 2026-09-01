using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams and records tenant-scoped truck geolocation.</summary>
[Authorize]
public class TrackingHub(
    ITruckGeolocationUpdater geolocationUpdater,
    ITenantUnitOfWork tenantUow,
    TrackingHubContext hubContext) : Hub<ITrackingHubClient>
{
    private const string TripGroupPrefix = "trip:";

    public override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId)
        {
            Context.Abort();
            return;
        }

        hubContext.AddClient(Context.ConnectionId, null);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId.ToString());
        await base.OnConnectedAsync();
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

    #region Geolocation Methods

    /// <summary>Records a position report for a truck assigned to the caller.</summary>
    [Authorize(Roles = TenantRoles.Driver)]
    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } driverId)
        {
            return;
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckGeolocation.TruckId);

        if (truck is null ||
            (truck.MainDriverId != driverId && truck.SecondaryDriverId != driverId))
        {
            return;
        }

        truckGeolocation.TenantId = tenantId;

        await Clients
            .Group(tenantId.ToString())
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }

    #endregion

    #region Tenant Subscription

    [Obsolete("Identity comes from JWT claims; remove once the driver app stops calling it.")]
    public Task RegisterTenant(string tenantId) => Task.CompletedTask;

    [Obsolete("Identity comes from JWT claims; remove once the driver app stops calling it.")]
    public Task UnregisterTenant(string tenantId) => Task.CompletedTask;

    #endregion

    #region Trip Subscription

    /// <summary>
    ///     Subscribe to updates for a specific trip.
    /// </summary>
    public async Task SubscribeToTrip(string tripId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{TripGroupPrefix}{tripId}");
    }

    /// <summary>
    ///     Unsubscribe from updates for a specific trip.
    /// </summary>
    public async Task UnsubscribeFromTrip(string tripId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{TripGroupPrefix}{tripId}");
    }

    #endregion
}
