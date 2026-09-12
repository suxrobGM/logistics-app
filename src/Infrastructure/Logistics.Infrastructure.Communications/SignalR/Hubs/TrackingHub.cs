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
        if (!await geolocationUpdater.CanDriverReportForTruckAsync(
                TenantId, truckGeolocation.TruckId, UserId))
        {
            return;
        }

        truckGeolocation.TenantId = TenantId;

        await Clients
            .Group(TenantId.ToString())
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }
}
