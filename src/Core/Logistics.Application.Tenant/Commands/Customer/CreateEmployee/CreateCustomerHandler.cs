namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateCustomerHandler : RequestHandler<CreateCustomerCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateCustomerHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateCustomerCommand req, CancellationToken cancellationToken)
    {
        var existingCustomer = await _tenantRepository.GetAsync<Customer>(i => i.Name == req.Name);
        
        if (existingCustomer is not null)
            return ResponseResult.CreateError($"A customer named '{req.Name}' already exists");
        
        await _tenantRepository.AddAsync(new Customer {Name = req.Name});
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
