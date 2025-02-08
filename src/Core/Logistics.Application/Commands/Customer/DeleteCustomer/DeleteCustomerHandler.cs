using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteCustomerHandler : RequestHandler<DeleteCustomerCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteCustomerHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteCustomerCommand req, CancellationToken cancellationToken)
    {
        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID {req.Id}");
        }
        
        _tenantUow.Repository<Customer>().Delete(customer);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
