using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Policies;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Dvir;

internal sealed class DvirReportAccess(ITenantUnitOfWork tenantUow, ICurrentUserService currentUser) : IDvirReportAccess
{
    public async Task<bool> CanFileForAsync(Guid driverId, CancellationToken ct)
    {
        if (currentUser.GetUserId() is not { } caller)
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
