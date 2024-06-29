using Logistics.Shared.Consts;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateInvoiceHandler : RequestHandler<CreateInvoiceCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateInvoiceCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find a load with ID '{req.LoadId}'");
        }
        
        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'");
        }

        var payment = new Payment
        {
            Method = req.PaymentMethod,
            PaymentFor = PaymentFor.Invoice,
            Amount = req.PaymentAmount
        };
        
        var invoice = new Invoice
        {
            CustomerId = req.CustomerId,
            LoadId = req.LoadId,
            Payment = payment
        };
        
        await _tenantUow.Repository<Invoice>().AddAsync(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
