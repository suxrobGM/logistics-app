using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class InvoiceMapper
{
    [MapperIgnoreSource(nameof(Invoice.Payments))]
    [MapperIgnoreSource(nameof(Invoice.LineItems))]
    [MapperIgnoreSource(nameof(Invoice.PaymentLinks))]
    [MapperIgnoreTarget(nameof(InvoiceDto.Type))]
    [MapperIgnoreTarget(nameof(InvoiceDto.Payments))]
    [MapperIgnoreTarget(nameof(InvoiceDto.LineItems))]
    [MapperIgnoreTarget(nameof(InvoiceDto.AmountPaid))]
    [MapperIgnoreTarget(nameof(InvoiceDto.AmountDue))]
    [MapperIgnoreTarget(nameof(InvoiceDto.LoadId))]
    [MapperIgnoreTarget(nameof(InvoiceDto.LoadNumber))]
    [MapperIgnoreTarget(nameof(InvoiceDto.CustomerId))]
    [MapperIgnoreTarget(nameof(InvoiceDto.Customer))]
    [MapperIgnoreTarget(nameof(InvoiceDto.FactoringStatus))]
    [MapperIgnoreTarget(nameof(InvoiceDto.EmployeeId))]
    [MapperIgnoreTarget(nameof(InvoiceDto.Employee))]
    [MapperIgnoreTarget(nameof(InvoiceDto.PeriodStart))]
    [MapperIgnoreTarget(nameof(InvoiceDto.PeriodEnd))]
    [MapProperty(nameof(Invoice.CreatedAt), nameof(InvoiceDto.CreatedDate))]
    [MapPropertyFromSource(nameof(InvoiceDto.TaxBreakdown), Use = nameof(MapTaxBreakdown))]
    private static partial InvoiceDto MapInvoice(Invoice entity);

    private static IEnumerable<InvoiceTaxLineDto> MapTaxBreakdown(Invoice entity) => entity.GetTaxBreakdown().ToDto();

    extension(Invoice entity)
    {
        public InvoiceDto ToDto()
        {
            return entity switch
            {
                LoadInvoice loadInvoice => loadInvoice.ToDto(
                    loadInvoice.Payments,
                    loadInvoice.LineItems,
                    loadInvoice.Load.Number,
                    loadInvoice.Customer.ToDto()),
                PayrollInvoice payrollInvoice => MapInvoice(payrollInvoice) with
                {
                    Type = InvoiceType.Payroll,
                    Payments = payrollInvoice.Payments.Select(i => i.ToDto()),
                    AmountPaid = payrollInvoice.AmountPaid,
                    AmountDue = payrollInvoice.AmountDue,
                    LineItems = payrollInvoice.LineItems.Select(i => i.ToDto()),
                    EmployeeId = payrollInvoice.EmployeeId,
                    Employee = payrollInvoice.Employee.ToDto(),
                    PeriodStart = payrollInvoice.PeriodStart,
                    PeriodEnd = payrollInvoice.PeriodEnd
                },
                _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
            };
        }
    }

    extension(LoadInvoice loadInvoice)
    {
        /// <summary>
        /// Takes the related rows explicitly so a batch of loads can map its invoices without a lazy
        /// SELECT per invoice for each collection.
        /// </summary>
        public InvoiceDto ToDto(
            IEnumerable<Payment> payments,
            IEnumerable<InvoiceLineItem> lineItems,
            long loadNumber,
            CustomerDto? customer)
        {
            var paymentList = payments.ToList();
            var amountPaid = Invoice.SumPaid(paymentList, loadInvoice.Total.Currency);
            return MapInvoice(loadInvoice) with
            {
                Type = InvoiceType.Load,
                Payments = paymentList.Select(i => i.ToDto()),
                AmountPaid = amountPaid,
                AmountDue = loadInvoice.Total.Amount - amountPaid,
                LineItems = lineItems.Select(i => i.ToDto()),
                LoadId = loadInvoice.LoadId,
                LoadNumber = loadNumber,
                CustomerId = loadInvoice.CustomerId,
                Customer = customer,
                FactoringStatus = loadInvoice.FactoringStatus
            };
        }
    }
}
