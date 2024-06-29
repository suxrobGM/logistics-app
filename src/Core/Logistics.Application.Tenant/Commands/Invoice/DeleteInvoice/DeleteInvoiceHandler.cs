namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteInvoiceHandler : RequestHandler<DeletePaymentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var invoice = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoice is null)
        {
            return Result.Fail($"Could not find an invoice with ID {req.Id}");
        }
        
        _tenantUow.Repository<Invoice>().Delete(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
