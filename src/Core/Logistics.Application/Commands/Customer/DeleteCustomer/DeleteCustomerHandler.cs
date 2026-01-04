using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteCustomerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.Id, ct);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID {req.Id}");
        }

        tenantUow.Repository<Customer>().Delete(customer);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
