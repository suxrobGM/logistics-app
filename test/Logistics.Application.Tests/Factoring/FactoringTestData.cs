using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Tests.Factoring;

internal static class FactoringTestData
{
    public static readonly Address Address = new()
    {
        Line1 = "1 Factor Way",
        City = "Dallas",
        State = "TX",
        ZipCode = "75201",
        Country = "US"
    };

    public static FactoringCompany Company(decimal advance = 90m, decimal fee = 3m) => new()
    {
        Name = "Acme Capital",
        SubmissionEmail = "schedules@acme.test",
        RemitToAddress = Address,
        AdvanceRatePercent = advance,
        FeePercent = fee
    };

    public static Customer Customer(FactoringCompany? factor, string name = "Big Broker") => new()
    {
        Name = name,
        FactoringCompanyId = factor?.Id,
        FactoringCompany = factor
    };

    public static LoadInvoice Invoice(
        Customer customer, long number = 1001, decimal total = 1000m, InvoiceStatus status = InvoiceStatus.Issued)
    {
        var money = new Money { Amount = total, Currency = "USD" };
        var load = new Load
        {
            Name = $"Load for {number}",
            Type = LoadType.GeneralFreight,
            Customer = customer,
            OriginAddress = Address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = Address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = money
        };

        return new LoadInvoice
        {
            Number = number,
            Status = status,
            Subtotal = money,
            TaxTotal = Money.Zero("USD"),
            Total = money,
            LoadId = load.Id,
            Load = load,
            CustomerId = customer.Id,
            Customer = customer
        };
    }

    /// <summary>A schedule with one submitted invoice of the given total, factored with <paramref name="factor"/>.</summary>
    public static (FactoringBatch Batch, FactoringBatchItem Item, LoadInvoice Invoice) SubmittedBatch(
        FactoringCompany factor, decimal total = 1000m)
    {
        var invoice = Invoice(Customer(factor), total: total);
        var batch = FactoringBatch.Submit(factor, [invoice], DateTime.UtcNow, null);
        return (batch, batch.Items[0], invoice);
    }
}
