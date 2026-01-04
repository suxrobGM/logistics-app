using Logistics.Application.Abstractions;
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

        customerEntity.Name = req.Name;
        tenantUow.Repository<Customer>().Update(customerEntity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
