using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Realtime;

/// <summary>
/// Persists the latest truck geolocation reported by a tracking client.
/// Wraps the in-process MediatR command so SignalR hubs and other adapters
/// can update geolocation without depending on the Application assembly.
/// </summary>
public interface ITruckGeolocationUpdater : IApplicationService
{
    Task UpdateAsync(TruckGeolocationDto geolocation, CancellationToken ct = default);
}
