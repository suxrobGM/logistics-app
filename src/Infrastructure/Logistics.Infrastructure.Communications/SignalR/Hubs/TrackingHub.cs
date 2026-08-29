using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Live truck geolocation. Authorized, and the tenant group is derived from the caller's JWT
///     tenant claim - never from a client-supplied id - so a client can only ever receive (or
///     broadcast to) its own tenant's group. Mirrors <see cref="CopilotHub"/>.
/// </summary>
[Authorize]
public class TrackingHub(
    ITruckGeolocationUpdater geolocationUpdater,
    TrackingHubContext hubContext) : Hub<ITrackingHubClient>
{
    private const string TripGroupPrefix = "trip:";

    public override async Task OnConnectedAsync()
    {
        var tenantId = TenantIdFromClaim();
        if (tenantId is null)
        {
            Context.Abort();
            return;
        }

        hubContext.AddClient(Context.ConnectionId, null);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
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

    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        var tenantId = TenantIdFromClaim();
        if (tenantId is null)
        {
            return;
        }

        // Broadcast to the caller's own tenant group from the claim, not to a client-supplied
        // TenantId, so a client cannot inject geolocation into another tenant's stream.
        await Clients
            .Group(tenantId)
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }

    #endregion

    #region Tenant Subscription

    // Kept for client compatibility; membership is established from the claim on connect and the
    // client-supplied id is deliberately ignored so a client cannot join another tenant's group.
    public Task RegisterTenant(string tenantId) => Task.CompletedTask;

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

    private string? TenantIdFromClaim() =>
        Context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
}
