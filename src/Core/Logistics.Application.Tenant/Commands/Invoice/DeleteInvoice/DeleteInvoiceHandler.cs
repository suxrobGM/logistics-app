namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteInvoiceHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteInvoiceHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var invoice = await _tenantRepository.GetAsync<Invoice>(req.Id);

        if (invoice is null)
            return ResponseResult.CreateError($"Could not find an invoice with ID {req.Id}");
        
        _tenantRepository.Delete(invoice);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
