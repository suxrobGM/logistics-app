using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Commands;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Services.Tracking;

internal sealed class TruckGeolocationUpdater(IMediator mediator) : ITruckGeolocationUpdater
{
    public Task UpdateAsync(TruckGeolocationDto geolocation, CancellationToken ct = default)
        => mediator.Send(new SetTruckGeolocationCommand(geolocation), ct);
}
