namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateInvoiceHandler : RequestHandler<UpdateInvoiceCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateInvoiceHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateInvoiceCommand req, CancellationToken cancellationToken)
    {
        var invoice = await _tenantRepository.GetAsync<Invoice>(req.Id);

        if (invoice is null)
            return ResponseResult.CreateError($"Could not find an invoice with ID '{req.Id}'");
        
        if (req.PaymentMethod.HasValue && invoice.Payment.Method != req.PaymentMethod)
        {
            invoice.Payment.Method = req.PaymentMethod.Value;
        }
        if (req.PaymentAmount.HasValue && invoice.Payment.Amount != req.PaymentAmount)
        {
            invoice.Payment.Amount = req.PaymentAmount.Value;
        }
        
        _tenantRepository.Update(invoice);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
