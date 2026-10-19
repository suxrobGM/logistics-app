using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class DeleteCustomerHandler(ITenantUnitOfWork tenantUow, IMasterUnitOfWork masterUow)
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
        var customerUserIds = await tenantUow.Repository<CustomerUser>().Query()
            .Where(cu => cu.CustomerId == customer.Id)
            .Select(cu => (Guid?)cu.Id)
            .ToListAsync(ct);

        if (customerUserIds.Count > 0)
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            var accessRepository = masterUow.Repository<UserTenantAccess>();
            var accesses = await accessRepository
                .GetListAsync(a => a.TenantId == tenantId && customerUserIds.Contains(a.CustomerUserId), ct);

            foreach (var access in accesses)
            {
                accessRepository.Delete(access);
            }

            await masterUow.SaveChangesAsync(ct);
        }

        tenantUow.Repository<Customer>().Delete(customer);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
