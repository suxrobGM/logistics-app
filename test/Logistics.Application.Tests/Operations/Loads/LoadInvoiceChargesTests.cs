using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadInvoiceChargesTests
{
    internal static Load NewLoad(decimal linehaul = 2000m)
    {
        var address = new Address { Line1 = "1 Main", City = "City", State = "TX", ZipCode = "00000", Country = "US" };
        var customer = new Customer { Name = "Acme Freight" };
        return new Load
        {
            Name = "Test",
            Type = LoadType.GeneralFreight,
            Customer = customer,
            CustomerId = customer.Id,
            OriginAddress = address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = new Money { Amount = linehaul, Currency = "USD" }
        };
    }

    private static LoadAccessorial Accessorial(Load load, string name, InvoiceLineItemType type, decimal rate,
        decimal quantity = 1, AccessorialUnit unit = AccessorialUnit.Flat) => new()
    {
        LoadId = load.Id,
        Load = load,
        Name = name,
        LineItemType = type,
        Unit = unit,
        Rate = rate,
        Quantity = quantity
    };

    [Fact]
    public void CreateInvoice_LinehaulOnly_HasSingleBaseRateLine()
    {
        var load = NewLoad();

        var invoice = LoadFactory.CreateInvoice(load);

        var line = Assert.Single(invoice.LineItems);
        Assert.Equal(InvoiceLineItemType.BaseRate, line.Type);
        Assert.Equal(2000m, invoice.Total.Amount);
    }

    [Fact]
    public void ReplaceLoadCharges_LinehaulFuelAndAccessorials_BuildsOneLineEachAndTotals()
    {
        var load = NewLoad();
        load.FuelSurcharge = 300m;
        load.Accessorials.Add(Accessorial(load, "Lumper", InvoiceLineItemType.Lumper, 150m));
        load.Accessorials.Add(Accessorial(load, "Extra stop", InvoiceLineItemType.Accessorial, 75m, 2,
            AccessorialUnit.PerStop));
        var invoice = LoadFactory.CreateInvoice(load);

        Assert.Collection(invoice.LineItems.OrderBy(li => li.Order),
            l => Assert.Equal(InvoiceLineItemType.BaseRate, l.Type),
            l =>
            {
                Assert.Equal(InvoiceLineItemType.FuelSurcharge, l.Type);
                Assert.Equal(300m, l.Total);
            },
            l =>
            {
                Assert.Equal("Extra stop", l.Description);
                Assert.Equal(75m, l.Amount.Amount);
                Assert.Equal(2, l.Quantity);
            },
            l => Assert.Equal(InvoiceLineItemType.Lumper, l.Type));
        Assert.Equal(2600m, invoice.Total.Amount);
    }

    [Fact]
    public void ReplaceLoadCharges_FractionalQuantity_BillsTotalOnOneLine()
    {
        var load = NewLoad();
        load.Accessorials.Add(Accessorial(load, "Detention", InvoiceLineItemType.Detention, 50m, 2.25m,
            AccessorialUnit.PerHour));

        var invoice = LoadFactory.CreateInvoice(load);

        var detention = invoice.LineItems.Single(l => l.Type == InvoiceLineItemType.Detention);
        Assert.Equal(1, detention.Quantity);
        Assert.Equal(112.50m, detention.Amount.Amount);
        Assert.Contains("2.25 x 50.00", detention.Description);
    }

    [Fact]
    public void ReplaceLoadCharges_Regenerate_KeepsHandAddedAndDrayageLines()
    {
        var load = NewLoad();
        var invoice = LoadFactory.CreateInvoice(load);
        var manual = new InvoiceLineItem
        {
            InvoiceId = invoice.Id, Description = "Tarp", Type = InvoiceLineItemType.Other,
            Amount = new Money { Amount = 40m, Currency = "USD" }, Order = 1
        };
        var demurrage = new InvoiceLineItem
        {
            InvoiceId = invoice.Id, Description = "Demurrage", Type = InvoiceLineItemType.Demurrage,
            Amount = new Money { Amount = 150m, Currency = "USD" }, Order = 2
        };
        invoice.LineItems.AddRange([manual, demurrage]);

        load.DeliveryCost = new Money { Amount = 2100m, Currency = "USD" };
        load.FuelSurcharge = 200m;
        var (removed, added) = invoice.ReplaceLoadCharges(load);
        invoice.RecalculateTotals();

        Assert.Single(removed);
        Assert.Equal(2, added.Count);
        Assert.Contains(manual, invoice.LineItems);
        Assert.Contains(demurrage, invoice.LineItems);
        Assert.Equal(2100m + 200m + 40m + 150m, invoice.Total.Amount);
        Assert.Equal([0, 1, 2, 3], invoice.LineItems.OrderBy(l => l.Order).Select(l => l.Order));
        Assert.Equal(InvoiceLineItemType.BaseRate, invoice.LineItems.OrderBy(l => l.Order).First().Type);
    }

    [Fact]
    public void ReplaceLoadCharges_AccessorialRemoved_DropsItsLine()
    {
        var load = NewLoad();
        var lumper = Accessorial(load, "Lumper", InvoiceLineItemType.Lumper, 150m);
        load.Accessorials.Add(lumper);
        var invoice = LoadFactory.CreateInvoice(load);

        load.Accessorials.Remove(lumper);
        invoice.ReplaceLoadCharges(load);

        Assert.DoesNotContain(invoice.LineItems, l => l.SourceId == lumper.Id);
        Assert.Single(invoice.LineItems);
    }
}
