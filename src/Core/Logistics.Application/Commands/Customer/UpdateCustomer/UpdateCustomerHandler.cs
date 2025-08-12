using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateCustomerHandler : IAppRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdateCustomerHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customerEntity = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customerEntity is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        customerEntity.Name = req.Name;
        _tenantUow.Repository<Customer>().Update(customerEntity);
        await _tenantUow.SaveChangesAsync(ct);
        return Result.Succeed();
    }
}
