namespace Logistics.Application.Tenant.Commands;

internal sealed class DeletePaymentHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeletePaymentHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantRepository.GetAsync<Payment>(req.Id);

        if (payment is null)
            return ResponseResult.CreateError($"Could not find a payment with ID {req.Id}");
        
        _tenantRepository.Delete(payment);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
