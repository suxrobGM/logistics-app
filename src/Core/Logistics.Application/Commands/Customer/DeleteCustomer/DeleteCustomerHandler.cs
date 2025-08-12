using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteCustomerHandler : RequestHandler<DeleteCustomerCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteCustomerHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result> Handle(DeleteCustomerCommand req, CancellationToken ct)
    {
        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID {req.Id}");
        }

        _tenantUow.Repository<Customer>().Delete(customer);
        await _tenantUow.SaveChangesAsync(ct);
        return Result.Succeed();
    }
}
