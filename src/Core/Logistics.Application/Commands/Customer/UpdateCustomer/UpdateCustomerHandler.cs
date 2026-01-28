using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateCustomerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customerEntity = await tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customerEntity is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        customerEntity.Name = PropertyUpdater.UpdateIfChanged(req.Name, customerEntity.Name);
        customerEntity.Email = PropertyUpdater.UpdateIfChanged(req.Email, customerEntity.Email);
        customerEntity.Phone = PropertyUpdater.UpdateIfChanged(req.Phone, customerEntity.Phone);
        customerEntity.Address = PropertyUpdater.UpdateIfChanged(req.Address, customerEntity.Address);
        customerEntity.Status = PropertyUpdater.UpdateIfChanged(req.Status, customerEntity.Status);
        customerEntity.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, customerEntity.Notes);
        tenantUow.Repository<Customer>().Update(customerEntity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
