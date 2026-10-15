using Logistics.Application.Modules.Operations.Containers.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Containers;

public class AddDrayageChargesHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);

    private static LoadInvoice Invoice() => new()
    {
        LoadId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Subtotal = Money.Zero("USD"),
        TaxTotal = Money.Zero("USD"),
        Total = new Money { Amount = 1200m, Currency = "USD" }
    };

    [Fact]
    public void BuildLines_AllCharges_PricesRateTimesDaysInInvoiceCurrency()
    {
        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            LastFreeDay = new DateOnly(2026, 10, 9),
            DemurrageRatePerDay = 150m,
            PickedUpAt = new DateTime(2026, 10, 11, 8, 0, 0, DateTimeKind.Utc),
            EmptyReturnDeadline = new DateOnly(2026, 10, 11),
            PerDiemRatePerDay = 100m,
            ChassisNumber = "DCLZ123456",
            ChassisProvider = ChassisProvider.Dcli,
            ChassisRatePerDay = 30m
        };
        var invoice = Invoice();

        var lines = AddDrayageChargesHandler.BuildLines(container, container.GetFreeTime(Now), invoice, 1);

        Assert.Collection(lines,
            demurrage =>
            {
                Assert.Equal(InvoiceLineItemType.Demurrage, demurrage.Type);
                Assert.Equal(150m, demurrage.Amount.Amount);
                Assert.Equal(2, demurrage.Quantity);
                Assert.Equal(1, demurrage.Order);
                Assert.Equal("USD", demurrage.Amount.Currency);
            },
            perDiem =>
            {
                Assert.Equal(InvoiceLineItemType.PerDiem, perDiem.Type);
                Assert.Equal(1, perDiem.Quantity);
                Assert.Equal(2, perDiem.Order);
            },
            chassis =>
            {
                Assert.Equal(InvoiceLineItemType.Chassis, chassis.Type);
                Assert.Equal(2, chassis.Quantity);
                Assert.Contains("DCLZ123456", chassis.Description);
            });
        Assert.All(lines, l => Assert.Equal(invoice.Id, l.InvoiceId));
    }

    [Fact]
    public void BuildLines_WithinFreeTimeAndOwnChassis_AddsNothing()
    {
        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            LastFreeDay = new DateOnly(2026, 10, 15),
            DemurrageRatePerDay = 150m,
            PickedUpAt = Now.AddDays(-1),
            ChassisProvider = ChassisProvider.Own,
            ChassisRatePerDay = 30m
        };

        var lines = AddDrayageChargesHandler.BuildLines(container, container.GetFreeTime(Now), Invoice(), 0);

        Assert.Empty(lines);
    }
}
