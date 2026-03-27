using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadInvoiceHandler(ITenantUnitOfWork tenantUow)
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

        var payment = new Payment
        {
            StripePaymentMethodId = req.StripePaymentMethodId,
            TenantId = tenant.Id,
            Amount = req.PaymentAmount,
            BillingAddress = tenant.CompanyAddress ?? new Address
            {
                Line1 = "N/A", City = "N/A", State = "N/A", ZipCode = "00000", Country = "US"
            }
        };

        var invoice = new LoadInvoice
        {
            Total = req.PaymentAmount,
            CustomerId = req.CustomerId,
            LoadId = req.LoadId
        };

        invoice.ApplyPayment(payment);
        await tenantUow.Repository<Invoice>().AddAsync(invoice);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
