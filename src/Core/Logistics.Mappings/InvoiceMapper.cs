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
                LoadId = loadInvoice.LoadId,
                LoadNumber = loadInvoice.Load.Number,
                CustomerId = loadInvoice.CustomerId,
                Customer = loadInvoice.Customer.ToDto()
            },
            SubscriptionInvoice subscriptionInvoice => new InvoiceDto
            {
                Id = subscriptionInvoice.Id,
                Type = InvoiceType.Load,
                Number = subscriptionInvoice.Number,
                Status = subscriptionInvoice.Status,
                Total = subscriptionInvoice.Total,
                DueDate = subscriptionInvoice.DueDate,
                Notes = subscriptionInvoice.Notes,
                StripeInvoiceId = subscriptionInvoice.StripeInvoiceId,
                Payments = subscriptionInvoice.Payments.Select(i => i.ToDto()),
                SubscriptionId = subscriptionInvoice.SubscriptionId,
                BillingPeriodStart = subscriptionInvoice.BillingPeriodStart,
                BillingPeriodEnd = subscriptionInvoice.BillingPeriodEnd
            },
            PayrollInvoice payrollInvoice => new InvoiceDto
            {
                Id = payrollInvoice.Id,
                Type = InvoiceType.Load,
                Number = payrollInvoice.Number,
                Status = payrollInvoice.Status,
                Total = payrollInvoice.Total,
                DueDate = payrollInvoice.DueDate,
                Notes = payrollInvoice.Notes,
                StripeInvoiceId = payrollInvoice.StripeInvoiceId,
                Payments = payrollInvoice.Payments.Select(i => i.ToDto()),
                EmployeeId = payrollInvoice.EmployeeId,
                Employee = payrollInvoice.Employee.ToDto(),
                PeriodStart = payrollInvoice.PeriodStart,
                PeriodEnd = payrollInvoice.PeriodEnd
            },
            _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
        };
    }
}
