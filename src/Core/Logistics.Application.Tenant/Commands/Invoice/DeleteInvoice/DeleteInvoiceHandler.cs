namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteInvoiceHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var invoice = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoice is null)
        {
            return ResponseResult.CreateError($"Could not find an invoice with ID {req.Id}");
        }
        
        _tenantUow.Repository<Invoice>().Delete(invoice);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
