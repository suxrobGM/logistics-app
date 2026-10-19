using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice entity)
    {
        return entity switch
        {
            LoadInvoice loadInvoice => loadInvoice.ToDto(
                loadInvoice.Payments,
                loadInvoice.LineItems,
                loadInvoice.Load.Number,
                loadInvoice.Customer.ToDto()),
            SubscriptionInvoice subscriptionInvoice => new InvoiceDto
            {
                Id = subscriptionInvoice.Id,
                Type = InvoiceType.Subscription,
                Number = subscriptionInvoice.Number,
                Status = subscriptionInvoice.Status,
                CreatedDate = subscriptionInvoice.CreatedAt,
                Subtotal = subscriptionInvoice.Subtotal,
                TaxTotal = subscriptionInvoice.TaxTotal,
                Total = subscriptionInvoice.Total,
                TaxBehavior = subscriptionInvoice.TaxBehavior,
                TaxBreakdown = subscriptionInvoice.GetTaxBreakdown().ToDto(),
                DueDate = subscriptionInvoice.DueDate,
                Notes = subscriptionInvoice.Notes,
                StripeInvoiceId = subscriptionInvoice.StripeInvoiceId,
                Payments = subscriptionInvoice.Payments.Select(i => i.ToDto()),
                LineItems = subscriptionInvoice.LineItems.Select(i => i.ToDto()),
                SentAt = subscriptionInvoice.SentAt,
                SentToEmail = subscriptionInvoice.SentToEmail,
                SubscriptionId = subscriptionInvoice.SubscriptionId,
                BillingPeriodStart = subscriptionInvoice.BillingPeriodStart,
                BillingPeriodEnd = subscriptionInvoice.BillingPeriodEnd
            },
            PayrollInvoice payrollInvoice => new InvoiceDto
            {
                Id = payrollInvoice.Id,
                Type = InvoiceType.Payroll,
                Number = payrollInvoice.Number,
                Status = payrollInvoice.Status,
                CreatedDate = payrollInvoice.CreatedAt,
                Subtotal = payrollInvoice.Subtotal,
                TaxTotal = payrollInvoice.TaxTotal,
                Total = payrollInvoice.Total,
                TaxBehavior = payrollInvoice.TaxBehavior,
                TaxBreakdown = payrollInvoice.GetTaxBreakdown().ToDto(),
                DueDate = payrollInvoice.DueDate,
                Notes = payrollInvoice.Notes,
                StripeInvoiceId = payrollInvoice.StripeInvoiceId,
                Payments = payrollInvoice.Payments.Select(i => i.ToDto()),
                LineItems = payrollInvoice.LineItems.Select(i => i.ToDto()),
                SentAt = payrollInvoice.SentAt,
                SentToEmail = payrollInvoice.SentToEmail,
                EmployeeId = payrollInvoice.EmployeeId,
                Employee = payrollInvoice.Employee.ToDto(),
                PeriodStart = payrollInvoice.PeriodStart,
                PeriodEnd = payrollInvoice.PeriodEnd
            },
            _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
        };
    }

    /// <summary>
    /// Takes the related rows explicitly so a batch of loads can map its invoices without a lazy
    /// SELECT per invoice for each collection.
    /// </summary>
    public static InvoiceDto ToDto(
        this LoadInvoice loadInvoice,
        IEnumerable<Payment> payments,
        IEnumerable<InvoiceLineItem> lineItems,
        long loadNumber,
        CustomerDto? customer) => new()
    {
        Id = loadInvoice.Id,
        Type = InvoiceType.Load,
        Number = loadInvoice.Number,
        Status = loadInvoice.Status,
        CreatedDate = loadInvoice.CreatedAt,
        Subtotal = loadInvoice.Subtotal,
        TaxTotal = loadInvoice.TaxTotal,
        Total = loadInvoice.Total,
        TaxBehavior = loadInvoice.TaxBehavior,
        TaxBreakdown = loadInvoice.GetTaxBreakdown().ToDto(),
        DueDate = loadInvoice.DueDate,
        Notes = loadInvoice.Notes,
        StripeInvoiceId = loadInvoice.StripeInvoiceId,
        Payments = payments.Select(i => i.ToDto()),
        LineItems = lineItems.Select(i => i.ToDto()),
        SentAt = loadInvoice.SentAt,
        SentToEmail = loadInvoice.SentToEmail,
        LoadId = loadInvoice.LoadId,
        LoadNumber = loadNumber,
        CustomerId = loadInvoice.CustomerId,
        Customer = customer,
        FactoringStatus = loadInvoice.FactoringStatus
    };
}
