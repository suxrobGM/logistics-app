using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Realtime;

/// <summary>
/// Persists the latest truck geolocation reported by a tracking client, and decides who may report
/// it. Wraps the in-process MediatR command so SignalR hubs and other adapters can update
/// geolocation without depending on the Application assembly.
/// </summary>
public interface ITruckGeolocationUpdater : IApplicationService
{
    Task UpdateAsync(TruckGeolocationDto geolocation, CancellationToken ct = default);

    /// <summary>
    /// Checks that the truck is one the driver is assigned to. Every ingress for a driver-reported
    /// position must go through this, so the rule lives in one place rather than per adapter.
    /// </summary>
    Task<bool> CanDriverReportForTruckAsync(
        Guid tenantId, Guid truckId, Guid driverId, CancellationToken ct = default);
}
