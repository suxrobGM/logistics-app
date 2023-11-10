using Logistics.Domain.Entities;
using Logistics.Shared.Enums;

namespace Logistics.DbMigrator.Core;

public static class InvoiceGenerator
{
    public static Invoice GenerateInvoice(Load load)
    {
        var payment = new Payment
        {
            Amount = load.DeliveryCost,
            Status = PaymentStatus.Paid,
            CreatedDate = DateTime.Today,
            PaymentDate = DateTime.Now,
            PaymentFor = PaymentFor.Invoice,
            Method = PaymentMethod.BankAccount,
            BillingAddress = "40 Crescent Ave, Boston, MA 02125, United States"
        };

        var invoice = new Invoice
        {
            CustomerId = load.CustomerId!,
            Customer = load.Customer!,
            LoadId = load.Id,
            Load = load,
            PaymentId = payment.Id,
            Payment = payment,
        };

        return invoice;
    }
}
