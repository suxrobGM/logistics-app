using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

internal sealed class TripAccess(ITenantUnitOfWork tenantUow) : ITripAccess
{
    public async Task<bool> CanUserViewTripAsync(
        Guid tenantId, Guid tripId, CancellationToken ct = default)
    {
        // Resolving against the caller's own tenant database is what makes another tenant's trip
        // id unfindable rather than merely unauthorized.
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        return await tenantUow.Repository<Trip>().GetByIdAsync(tripId, ct) is not null;
    }
}
