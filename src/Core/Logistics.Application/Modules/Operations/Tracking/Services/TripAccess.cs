using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

internal sealed class TripAccess(
    ITenantUnitOfWork tenantUow,
    IUserPermissionService userPermissions) : ITripAccess
{
    public async Task<bool> CanUserViewTripAsync(
        Guid tenantId, Guid tripId, Guid userId, CancellationToken ct = default)
    {
        // Resolving against the caller's own tenant database is what makes another tenant's trip
        // id unfindable rather than merely unauthorized.
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(tripId, ct);
        if (trip is null)
        {
            return false;
        }

        // Load.Manage is the dispatch desk: TripController gates its write endpoints on it and the
        // Driver role ships with Load.View only.
        if (await userPermissions.HasPermissionAsync(userId, tenantId, Permission.Load.Manage, ct))
        {
            return true;
        }

        // An unassigned trip has no driver yet, so nobody outside dispatch qualifies.
        if (trip.TruckId is not { } truckId)
        {
            return false;
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);
        return truck?.IsDrivenBy(userId) == true;
    }
}
