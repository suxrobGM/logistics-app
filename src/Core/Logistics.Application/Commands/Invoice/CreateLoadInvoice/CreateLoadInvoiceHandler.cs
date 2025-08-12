using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadInvoiceHandler : RequestHandler<CreateLoadInvoiceCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreateLoadInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreateLoadInvoiceCommand req, CancellationToken ct)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null) return Result.Fail($"Could not find a load with ID '{req.LoadId}'");

        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId);

        if (customer is null) return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'");

        var paymentMethod = await _tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.PaymentMethodId);

        if (paymentMethod is null)
            return Result.Fail($"Could not find a payment method with ID '{req.PaymentMethodId}'");

        var tenant = _tenantUow.GetCurrentTenant();

        var payment = new Payment
        {
            MethodId = paymentMethod.Id,
            TenantId = tenant.Id,
            Amount = req.PaymentAmount,
            BillingAddress = tenant.CompanyAddress
        };

        var invoice = new LoadInvoice
        {
            Total = req.PaymentAmount,
            CustomerId = req.CustomerId,
            LoadId = req.LoadId
        };

        invoice.ApplyPayment(payment);
        await _tenantUow.Repository<Invoice>().AddAsync(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
