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
            LoadInvoice loadInvoice => new InvoiceDto
            {
                Id = loadInvoice.Id,
                Type = InvoiceType.Load,
                Number = loadInvoice.Number,
                Status = loadInvoice.Status,
                CreatedDate = loadInvoice.CreatedAt,
                Total = loadInvoice.Total,
                DueDate = loadInvoice.DueDate,
                Notes = loadInvoice.Notes,
                StripeInvoiceId = loadInvoice.StripeInvoiceId,
                Payments = loadInvoice.Payments.Select(i => i.ToDto()),
                LineItems = loadInvoice.LineItems.Select(i => i.ToDto()),
                SentAt = loadInvoice.SentAt,
                SentToEmail = loadInvoice.SentToEmail,
                LoadId = loadInvoice.LoadId,
                LoadNumber = loadInvoice.Load.Number,
                CustomerId = loadInvoice.CustomerId,
                Customer = loadInvoice.Customer.ToDto()
            },
            SubscriptionInvoice subscriptionInvoice => new InvoiceDto
            {
                Id = subscriptionInvoice.Id,
                Type = InvoiceType.Subscription,
                Number = subscriptionInvoice.Number,
                Status = subscriptionInvoice.Status,
                CreatedDate = subscriptionInvoice.CreatedAt,
                Total = subscriptionInvoice.Total,
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
                Total = payrollInvoice.Total,
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
}
