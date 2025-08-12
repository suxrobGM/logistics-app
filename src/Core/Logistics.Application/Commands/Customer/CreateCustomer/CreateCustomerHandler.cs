using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateCustomerHandler : IAppRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public CreateCustomerHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand req, CancellationToken ct)
    {
        var existingCustomer = await _tenantUow.Repository<Customer>().GetAsync(i => i.Name == req.Name, ct);

        if (existingCustomer is not null)
        {
            return Result<CustomerDto>.Fail($"A customer named '{req.Name}' already exists");
        }

        var newCustomer = new Customer
        {
            Name = req.Name
        };
        await _tenantUow.Repository<Customer>().AddAsync(newCustomer, ct);
        await _tenantUow.SaveChangesAsync(ct);
        return Result<CustomerDto>.Succeed(newCustomer.ToDto());
    }
}
