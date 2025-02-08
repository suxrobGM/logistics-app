using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateCustomerHandler : RequestHandler<CreateCustomerCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateCustomerHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateCustomerCommand req, CancellationToken cancellationToken)
    {
        var existingCustomer = await _tenantUow.Repository<Customer>().GetAsync(i => i.Name == req.Name);

        if (existingCustomer is not null)
        {
            return Result.Fail($"A customer named '{req.Name}' already exists");
        }
        
        await _tenantUow.Repository<Customer>().AddAsync(new Customer {Name = req.Name});
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
