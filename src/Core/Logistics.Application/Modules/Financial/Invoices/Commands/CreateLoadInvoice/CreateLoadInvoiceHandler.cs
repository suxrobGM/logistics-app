using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class CreateLoadInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IInvoiceTaxApplier taxApplier)
    : IRequestHandler<CreateLoadInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateLoadInvoiceCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        if (load is null)
        {
            return Result<Guid>.Fail($"Could not find a load with ID '{req.LoadId}'");
        }

        var existingInvoice = await tenantUow.Repository<LoadInvoice>()
            .GetAsync(i => i.LoadId == req.LoadId, ct);
        if (existingInvoice is not null)
        {
            return Result<Guid>.Fail(
                $"An invoice already exists for load '{load.Number}'.",
                ErrorCodes.LoadInvoiceExists);
        }

        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId, ct);

        if (customer is null)
        {
            return Result<Guid>.Fail($"Could not find a customer with ID '{req.CustomerId}'");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var currency = load.DeliveryCost.Currency;

        // The tax applier recomputes the totals from the lines, so an invoice without them bills zero.
        var invoice = new LoadInvoice
        {
            Subtotal = Money.Zero(currency),
            TaxTotal = Money.Zero(currency),
            Total = Money.Zero(currency),
            CustomerId = req.CustomerId,
            Customer = customer,
            LoadId = req.LoadId,
            Load = load
        };
        invoice.ReplaceLoadCharges(load);

        if (req.RecordPayment)
        {
            var payment = new Payment
            {
                StripePaymentMethodId = req.StripePaymentMethodId,
                TenantId = tenant.Id,
                Amount = new() { Amount = req.PaymentAmount, Currency = currency },
                BillingAddress = tenant.CompanyAddress
            };

            invoice.ApplyPayment(payment);
        }

        await taxApplier.ApplyAsync(invoice, ct);

        await tenantUow.Repository<Invoice>().AddAsync(invoice, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(invoice.Id);
    }
}
