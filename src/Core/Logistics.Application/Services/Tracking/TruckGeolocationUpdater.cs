using Logistics.Application.Abstractions.Realtime;
using Logistics.Shared.Models;
using MediatR;
using Logistics.Application.Modules.Operations.Trucks.Commands;

namespace Logistics.Application.Services.Tracking;

internal sealed class TruckGeolocationUpdater(IMediator mediator) : ITruckGeolocationUpdater
{
    public Task UpdateAsync(TruckGeolocationDto geolocation, CancellationToken ct = default)
        => mediator.Send(new SetTruckGeolocationCommand(geolocation), ct);
}
