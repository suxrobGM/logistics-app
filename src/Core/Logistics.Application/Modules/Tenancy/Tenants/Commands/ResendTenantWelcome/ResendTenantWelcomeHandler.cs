using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Tenancy.Tenants.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

internal sealed class ResendTenantWelcomeHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ITenantWelcomeEmailSender welcomeEmailSender)
    : IRequestHandler<ResendTenantWelcomeCommand, Result>
{
    public async Task<Result> Handle(ResendTenantWelcomeCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);

        if (tenant is null)
        {
            return Result.Fail("Tenant not found", ErrorCodes.NotFound);
        }

        tenantUow.SetCurrentTenant(tenant);
        var ownerId = await tenantUow.Repository<Employee>().Query()
            .Where(e => e.Role != null && e.Role.Name == TenantRoles.Owner)
            .OrderBy(e => e.JoinedDate)
            .Select(e => (Guid?)e.Id)
            .FirstOrDefaultAsync(ct);

        var owner = ownerId is null ? null : await masterUow.Repository<User>().GetByIdAsync(ownerId.Value, ct);

        if (owner is null)
        {
            return Result.Fail("No owner found for this tenant");
        }

        await welcomeEmailSender.SendAsync(owner, tenant);

        return Result.Ok();
    }
}
