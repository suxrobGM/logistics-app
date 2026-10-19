using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class UpdateCustomerHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    ICustomerPortalAccess portalAccess)
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customerEntity = await tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customerEntity is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        var nameChanged = !string.IsNullOrEmpty(req.Name)
                          && !string.Equals(req.Name, customerEntity.Name, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && await IsNameTakenAsync(req.Name, req.Id, ct))
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

    private Task<bool> IsNameTakenAsync(string name, Guid customerId, CancellationToken ct) =>
        tenantUow.Repository<Customer>().Query()
            .AnyAsync(c => c.Id != customerId && c.Name.ToLower() == name.ToLower(), ct);

    private async Task RenamePortalAccessAsync(Customer customer, CancellationToken ct)
    {
        var accesses = await portalAccess.ListForCustomerAsync(customer.Id, ct);
        if (accesses.Count == 0)
        {
            return;
        }

        foreach (var access in accesses)
        {
            access.CustomerName = customer.Name;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
