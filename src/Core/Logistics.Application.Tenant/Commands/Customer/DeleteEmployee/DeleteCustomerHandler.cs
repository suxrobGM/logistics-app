namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteCustomerHandler : RequestHandler<DeleteCustomerCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteCustomerHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteCustomerCommand req, CancellationToken cancellationToken)
    {
        var customer = await _tenantRepository.GetAsync<Customer>(req.Id);

        if (customer is null)
            return ResponseResult.CreateError($"Could not find a customer with ID {req.Id}");
        
        _tenantRepository.Delete(customer);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
