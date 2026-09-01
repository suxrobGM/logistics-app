using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Operations.Trucks.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

internal sealed class TruckGeolocationUpdater(IMediator mediator, ITenantUnitOfWork tenantUow)
    : ITruckGeolocationUpdater
{
    public Task UpdateAsync(TruckGeolocationDto geolocation, CancellationToken ct = default)
        => mediator.Send(new SetTruckGeolocationCommand(geolocation), ct);

    public async Task<bool> CanDriverReportForTruckAsync(
        Guid tenantId, Guid truckId, Guid driverId, CancellationToken ct = default)
    {
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);

        return truck?.IsDrivenBy(driverId) == true;
    }
}
