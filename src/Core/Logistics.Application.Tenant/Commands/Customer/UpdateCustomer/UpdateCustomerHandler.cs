namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateCustomerHandler : RequestHandler<UpdateCustomerCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateCustomerHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateCustomerCommand req, CancellationToken cancellationToken)
    {
        var customerEntity = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id);

        if (customerEntity is null)
        {
            return ResponseResult.CreateError($"Could not find a customer with ID '{req.Id}'");
        }

        customerEntity.Name = req.Name;
        _tenantUow.Repository<Customer>().Update(customerEntity);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
