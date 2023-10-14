namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateCustomerHandler : RequestHandler<UpdateCustomerCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateCustomerHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateCustomerCommand req, CancellationToken cancellationToken)
    {
        var customerEntity = await _tenantRepository.GetAsync<Customer>(req.Id);

        if (customerEntity is null)
            return ResponseResult.CreateError($"Could not find a customer with ID '{req.Id}'");

        customerEntity.Name = req.Name;
        _tenantRepository.Update(customerEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
