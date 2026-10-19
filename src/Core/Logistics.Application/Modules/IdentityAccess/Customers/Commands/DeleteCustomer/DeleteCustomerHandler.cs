using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class DeleteCustomerHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    ICustomerPortalAccess portalAccess)
    : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        // The delete cascades to the customer's portal users; their master access rows go first.
        var accesses = await portalAccess.ListForCustomerAsync(customer.Id, ct);
        if (accesses.Count > 0)
        {
            foreach (var access in accesses)
            {
                masterUow.Repository<UserTenantAccess>().Delete(access);
            }

            await masterUow.SaveChangesAsync(ct);
        }

        tenantUow.Repository<Customer>().Delete(customer);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
