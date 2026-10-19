using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Commands;

internal sealed class UpdateCustomerUserHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    ICustomerPortalAccess portalAccess)
    : IRequestHandler<UpdateCustomerUserCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerUserCommand req, CancellationToken ct)
    {
        var customerUser = await tenantUow.Repository<CustomerUser>().GetByIdAsync(req.Id, ct);

        if (customerUser is null)
        {
            return Result.Fail($"Customer user with ID '{req.Id}' not found.");
        }

        if (req.DisplayName is not null)
        {
            customerUser.DisplayName = req.DisplayName;
        }

        if (req.IsActive is { } isActive && isActive != customerUser.IsActive)
        {
            if (isActive && await portalAccess.HasActiveAsync(customerUser.UserId, ct))
            {
                return Result.Fail(CustomerPortalAccess.AlreadyActiveMessage);
            }

            customerUser.IsActive = isActive;

            var access = await portalAccess.FindAsync(customerUser.Id, ct);

            if (access is not null)
            {
                access.IsActive = isActive;
                await masterUow.SaveChangesAsync(ct);
            }
        }

        tenantUow.Repository<CustomerUser>().Update(customerUser);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
