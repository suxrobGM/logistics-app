namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateCustomerHandler : RequestHandler<CreateCustomerCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateCustomerHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateCustomerCommand req, CancellationToken cancellationToken)
    {
        var existingCustomer = await _tenantUow.Repository<Customer>().GetAsync(i => i.Name == req.Name);

        if (existingCustomer is not null)
        {
            return ResponseResult.CreateError($"A customer named '{req.Name}' already exists");
        }
        
        await _tenantUow.Repository<Customer>().AddAsync(new Customer {Name = req.Name});
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
