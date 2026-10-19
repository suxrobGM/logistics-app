using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class UpdateCustomerHandler(ITenantUnitOfWork tenantUow, IMasterUnitOfWork masterUow)
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customerEntity = await tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customerEntity is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(req.Name) && await tenantUow.Repository<Customer>().Query()
                .AnyAsync(c => c.Id != req.Id && c.Name.ToLower() == req.Name.ToLower(), ct))
        {
            return Result.Fail($"A customer named '{req.Name}' already exists");
        }

        var previousName = customerEntity.Name;
        customerEntity.Name = PropertyUpdater.UpdateIfChanged(req.Name, customerEntity.Name);
        customerEntity.Email = PropertyUpdater.UpdateIfChanged(req.Email, customerEntity.Email);
        customerEntity.Phone = PropertyUpdater.UpdateIfChanged(req.Phone, customerEntity.Phone);
        customerEntity.Address = PropertyUpdater.UpdateIfChanged(req.Address, customerEntity.Address);
        customerEntity.Status = PropertyUpdater.UpdateIfChanged(req.Status, customerEntity.Status);
        customerEntity.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, customerEntity.Notes);
        customerEntity.TaxId = PropertyUpdater.UpdateIfChanged(req.TaxId, customerEntity.TaxId);
        customerEntity.IsVatExempt = PropertyUpdater.UpdateIfChanged(req.IsVatExempt, customerEntity.IsVatExempt);
        customerEntity.FactoringCompanyId = req.FactoringCompanyId;

        tenantUow.Repository<Customer>().Update(customerEntity);
        await tenantUow.SaveChangesAsync(ct);

        if (customerEntity.Name != previousName)
        {
            await RenamePortalAccessAsync(customerEntity, ct);
        }

        return Result.Ok();
    }

    private async Task RenamePortalAccessAsync(Customer customer, CancellationToken ct)
    {
        var customerUserIds = await tenantUow.Repository<CustomerUser>().Query()
            .Where(cu => cu.CustomerId == customer.Id)
            .Select(cu => (Guid?)cu.Id)
            .ToListAsync(ct);

        if (customerUserIds.Count == 0)
        {
            return;
        }

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var accesses = await masterUow.Repository<UserTenantAccess>()
            .GetListAsync(a => a.TenantId == tenantId && customerUserIds.Contains(a.CustomerUserId), ct);

        foreach (var access in accesses)
        {
            access.CustomerName = customer.Name;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
