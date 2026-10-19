using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Commands;

internal sealed class DeleteCustomerUserHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    ICustomerPortalAccess portalAccess)
    : IRequestHandler<DeleteCustomerUserCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerUserCommand req, CancellationToken ct)
    {
        var customerUser = await tenantUow.Repository<CustomerUser>().GetByIdAsync(req.Id, ct);

        if (customerUser is null)
        {
            return Result.Fail($"Could not find a customer user with ID '{req.Id}'");
        }

        // Revoke the master access row first, so a failure part-way leaves the user locked out rather than in.
        var access = await portalAccess.FindAsync(customerUser.Id, ct);

        if (access is not null)
        {
            masterUow.Repository<UserTenantAccess>().Delete(access);
            await masterUow.SaveChangesAsync(ct);
        }

        tenantUow.Repository<CustomerUser>().Delete(customerUser);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
