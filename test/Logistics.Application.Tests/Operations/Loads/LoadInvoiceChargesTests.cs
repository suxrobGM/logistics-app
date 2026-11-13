using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadInvoiceChargesTests
{
    internal static Load NewLoad(decimal linehaul = 2000m) =>
        TestData.Load(deliveryCost: linehaul, customer: new Customer { Name = "Acme Freight" });

    internal static LoadAccessorial Accessorial(Load load, string name, InvoiceLineItemType type, decimal rate,
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

        var invoice = LoadInvoice.Create(load);

        var line = Assert.Single(invoice.LineItems);
        Assert.Equal(InvoiceLineItemType.BaseRate, line.Type);
        Assert.Equal(2000m, invoice.Total);
    }

    [Fact]
    public void ReplaceLoadCharges_LinehaulFuelAndAccessorials_BuildsOneLineEachAndTotals()
    {
        var load = NewLoad();
        load.FuelSurcharge = 300m;
        load.Accessorials.Add(Accessorial(load, "Lumper", InvoiceLineItemType.Lumper, 150m));
        load.Accessorials.Add(Accessorial(load, "Extra stop", InvoiceLineItemType.Accessorial, 75m, 2,
            AccessorialUnit.PerStop));
        var invoice = LoadInvoice.Create(load);

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
                Assert.Equal(75m, l.Amount);
                Assert.Equal(2, l.Quantity);
            },
            l => Assert.Equal(InvoiceLineItemType.Lumper, l.Type));
        Assert.Equal(2600m, invoice.Total);
    }

    [Fact]
    public void ReplaceLoadCharges_FractionalQuantity_KeepsQuantityAndRate()
    {
        var load = NewLoad();
        load.Accessorials.Add(Accessorial(load, "Detention", InvoiceLineItemType.Detention, 50m, 2.25m,
            AccessorialUnit.PerHour));

        var invoice = LoadInvoice.Create(load);

        var detention = invoice.LineItems.Single(l => l.Type == InvoiceLineItemType.Detention);
        Assert.Equal(2.25m, detention.Quantity);
        Assert.Equal(50m, detention.Amount);
        Assert.Equal(112.50m, detention.Total);
    }

    [Fact]
    public void ReplaceLoadCharges_Regenerate_KeepsHandAddedLines()
    {
        var load = NewLoad();
        var invoice = LoadInvoice.Create(load);
        var manual = new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = "Tarp",
            Type = InvoiceLineItemType.Other,
            Amount = 40m,
            Order = 1
        };
        var storage = new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = "Storage",
            Type = InvoiceLineItemType.Storage,
            Amount = 150m,
            Order = 2
        };
        invoice.LineItems.AddRange([manual, storage]);

        load.DeliveryCost = 2100m;
        load.FuelSurcharge = 200m;
        var (removed, added) = invoice.ReplaceLoadCharges(load);
        invoice.RecalculateTotals();

        Assert.Single(removed);
        Assert.Equal(2, added.Count);
        Assert.Contains(manual, invoice.LineItems);
        Assert.Contains(storage, invoice.LineItems);
        Assert.Equal(2100m + 200m + 40m + 150m, invoice.Total);
        Assert.Equal([0, 1, 2, 3], invoice.LineItems.OrderBy(l => l.Order).Select(l => l.Order));
        Assert.Equal(InvoiceLineItemType.BaseRate, invoice.LineItems.OrderBy(l => l.Order).First().Type);
    }

    [Fact]
    public void ReplaceLoadCharges_AccessorialRemoved_DropsItsLine()
    {
        var load = NewLoad();
        var lumper = Accessorial(load, "Lumper", InvoiceLineItemType.Lumper, 150m);
        load.Accessorials.Add(lumper);
        var invoice = LoadInvoice.Create(load);

        load.Accessorials.Remove(lumper);
        invoice.ReplaceLoadCharges(load);

        Assert.DoesNotContain(invoice.LineItems, l => l.SourceId == lumper.Id);
        Assert.Single(invoice.LineItems);
    }
}
