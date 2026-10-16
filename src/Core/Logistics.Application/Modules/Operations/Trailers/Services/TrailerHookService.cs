using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trailers.Services;

internal sealed class TrailerHookService(ITenantUnitOfWork tenantUow) : ITrailerHookService
{
    public async Task HookAsync(Truck truck, Trailer? trailer, CancellationToken ct = default)
    {
        if (trailer is null)
        {
            truck.HookTrailer(null);
            return;
        }

        // Checked before the save below, so a rejected hook leaves the other truck alone.
        trailer.EnsureCanBeHooked();

        var previousTruck = await tenantUow.Repository<Truck>()
            .GetAsync(i => i.CurrentTrailerId == trailer.Id && i.Id != truck.Id, ct);
        if (previousTruck is not null)
        {
            previousTruck.HookTrailer(null);

            // Postgres checks the unique index per statement, so the unhook must land before the hook.
            await tenantUow.SaveChangesAsync(ct);
        }

        truck.HookTrailer(trailer);
    }

    public async Task UnhookUnlessActiveAsync(Trailer trailer, CancellationToken ct = default)
    {
        if (trailer.Status == TrailerStatus.Active)
        {
            return;
        }

        var truck = await tenantUow.Repository<Truck>().GetAsync(i => i.CurrentTrailerId == trailer.Id, ct);
        truck?.HookTrailer(null);
    }
}
