using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Policies;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir;

internal static class DvirReportAccess
{
    /// <summary>
    /// Drivers hold <c>Dvir.Manage</c> to file their own, which alone would let any of them sign a
    /// compliance record as a colleague. <c>Dvir.Review</c> is what buys acting on someone else's.
    /// </summary>
    public static async Task<bool> CanFileForAsync(
        ITenantUnitOfWork tenantUow,
        Guid? callerId,
        Guid driverId,
        CancellationToken ct)
    {
        if (callerId is not { } caller)
        {
            return false;
        }

        if (caller == driverId)
        {
            return true;
        }

        // The tenant's own claims, not TenantRolePermissions - roles are editable per tenant.
        return await tenantUow.Repository<Employee>()
            .Query()
            .Where(e => e.Id == caller)
            .SelectMany(e => e.Role!.Claims)
            .AnyAsync(
                c => c.ClaimType == CustomClaimTypes.Permission
                     && c.ClaimValue == Permission.Dvir.Review,
                ct);
    }
}
