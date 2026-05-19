using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class CreateLoadInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IAppRequestHandler<CreateLoadInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        CreateLoadInvoiceCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find a load with ID '{req.LoadId}'");
        }

        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId);

        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var currency = (tenant.Settings?.Currency ?? CurrencyCode.USD).ToString();

        var payment = new Payment
        {
            StripePaymentMethodId = req.StripePaymentMethodId,
            TenantId = tenant.Id,
            Amount = new() { Amount = req.PaymentAmount, Currency = currency },
            BillingAddress = tenant.CompanyAddress
        };

        var amount = new Money { Amount = req.PaymentAmount, Currency = currency };
        var invoice = new LoadInvoice
        {
            Subtotal = amount,
            TaxTotal = Money.Zero(currency),
            Total = amount,
            CustomerId = req.CustomerId,
            Customer = customer,
            LoadId = req.LoadId
        };

        invoice.ApplyPayment(payment);

        await taxApplier.ApplyAsync(invoice, ct);

        await tenantUow.Repository<Invoice>().AddAsync(invoice);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
