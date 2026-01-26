using Logistics.Application.Commands;
using Logistics.Application.Hubs;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

public class LiveTrackingHub(
    IMediator mediator,
    LiveTrackingHubContext hubContext) : Hub<ILiveTrackingHubClient>
{
    // Group name prefixes for organizing subscriptions
    private const string TripGroupPrefix = "trip:";
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public override Task OnConnectedAsync()
    {
        hubContext.AddClient(Context.ConnectionId, null);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var geolocationData = hubContext.GetGeolocationData(Context.ConnectionId);

        if (geolocationData != null)
        {
            await mediator.Send(new SetTruckGeolocationCommand(geolocationData));
        }

        hubContext.RemoveClient(Context.ConnectionId);
    }

    #region Geolocation Methods

    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        await Clients
            .Group(truckGeolocation.TenantId.ToString())
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }

    #endregion

    #region Tenant Subscription

    public async Task RegisterTenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }

    public async Task UnregisterTenant(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }

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

    #region Dispatch Board Subscription

    /// <summary>
    ///     Subscribe to dispatch board updates for a tenant.
    /// </summary>
    public async Task SubscribeToDispatchBoard(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{DispatchBoardGroupPrefix}{tenantId}");
    }

    /// <summary>
    ///     Unsubscribe from dispatch board updates.
    /// </summary>
    public async Task UnsubscribeFromDispatchBoard(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{DispatchBoardGroupPrefix}{tenantId}");
    }

    #endregion
}
