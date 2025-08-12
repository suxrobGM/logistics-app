using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateCustomerHandler : RequestHandler<UpdateCustomerCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateCustomerHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateCustomerCommand req, CancellationToken cancellationToken)
    {
        var customerEntity = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id);

        if (customerEntity is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.Id}'");
        }

        customerEntity.Name = req.Name;
        _tenantUow.Repository<Customer>().Update(customerEntity);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
