using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class SettlementLinesTests
{
    private static PayrollInvoice Settlement() => new()
    {
        EmployeeId = Guid.NewGuid(),
        Subtotal = Money.Zero("USD"),
        TaxTotal = Money.Zero("USD"),
        Total = Money.Zero("USD"),
        PeriodStart = new DateTime(2026, 10, 4),
        PeriodEnd = new DateTime(2026, 10, 10)
    };

    private static RecurringDeduction Deduction(PayDeductionType type, decimal amount, decimal? target = null) => new()
    {
        Type = type,
        Description = type.ToString(),
        Amount = amount,
        EscrowTarget = target
    };

    [Fact]
    public void ReplaceSettlementLines_GrossAndDeductions_TotalIsNet()
    {
        var payroll = Settlement();
        var advance = new CashAdvance { Amount = new Money { Amount = 200m, Currency = "USD" }, Reason = "Tires" };
        var fuel = new FuelCardTransaction
        {
            ProviderType = FuelCardProviderType.Wex,
            ExternalTransactionId = "tx-1",
            TransactionDate = new DateTime(2026, 10, 6),
            Amount = new Money { Amount = 450m, Currency = "USD" },
            MerchantName = "Pilot",
            MerchantCity = "Amarillo"
        };

        payroll.ReplaceSettlementLines(new SettlementInputs(
            5000m, "88% of linehaul on 3 load(s)",
            [Deduction(PayDeductionType.TruckLease, 600m), Deduction(PayDeductionType.Insurance, 250m)],
            0, [advance], [fuel]));

        Assert.Equal(5000m - 600m - 250m - 200m - 450m, payroll.Total.Amount);
        Assert.Collection(payroll.LineItems.OrderBy(l => l.Order),
            l => Assert.Equal((InvoiceLineItemType.BasePay, 5000m), (l.Type, l.Total)),
            l => Assert.Equal((InvoiceLineItemType.Deduction, -600m), (l.Type, l.Total)),
            l => Assert.Equal((InvoiceLineItemType.Deduction, -250m), (l.Type, l.Total)),
            l =>
            {
                Assert.Equal((InvoiceLineItemType.CashAdvance, -200m), (l.Type, l.Total));
                Assert.Equal(advance.Id, l.SourceId);
            },
            l =>
            {
                Assert.Equal((InvoiceLineItemType.FuelCard, -450m), (l.Type, l.Total));
                Assert.Contains("Pilot, Amarillo", l.Description);
            });
    }

    [Theory]
    [InlineData(0, 100, 100)]       // no balance yet
    [InlineData(2450, 100, 50)]     // tops up to the target
    [InlineData(2500, 100, 0)]      // target reached: no line
    public void ReplaceSettlementLines_Escrow_StopsAtTarget(decimal balance, decimal amount, decimal expected)
    {
        var payroll = Settlement();

        payroll.ReplaceSettlementLines(new SettlementInputs(
            1000m, "Gross", [Deduction(PayDeductionType.Escrow, amount, 2500m)], balance, [], []));

        var escrow = payroll.LineItems.SingleOrDefault(l => l.Type == InvoiceLineItemType.Escrow);
        Assert.Equal(expected, -(escrow?.Total ?? 0));
    }

    [Fact]
    public void ReplaceSettlementLines_EscrowWithoutTarget_AlwaysContributes()
    {
        var payroll = Settlement();

        payroll.ReplaceSettlementLines(new SettlementInputs(
            1000m, "Gross", [Deduction(PayDeductionType.Escrow, 100m)], 10_000m, [], []));

        Assert.Equal(900m, payroll.Total.Amount);
    }

    [Fact]
    public void ReplaceSettlementLines_Rebuild_KeepsHandAddedLines()
    {
        var payroll = Settlement();
        payroll.ReplaceSettlementLines(new SettlementInputs(
            1000m, "Gross", [Deduction(PayDeductionType.EldFee, 30m)], 0, [], []));
        payroll.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = payroll.Id, Description = "Lumper paid by driver", Type = InvoiceLineItemType.Reimbursement,
            Amount = new Money { Amount = 120m, Currency = "USD" }, Order = 5
        });
        payroll.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = payroll.Id, Description = "Damaged pallet", Type = InvoiceLineItemType.Deduction,
            Amount = new Money { Amount = PayrollInvoice.SignedLineAmount(InvoiceLineItemType.Deduction, 80m), Currency = "USD" },
            Order = 6
        });

        var (removed, added) = payroll.ReplaceSettlementLines(SettlementInputs.GrossOnly(1500m, "Gross"));

        Assert.Equal(2, removed.Count);
        Assert.Single(added);
        Assert.Equal(1500m + 120m - 80m, payroll.Total.Amount);
    }

    [Theory]
    [InlineData(InvoiceLineItemType.Deduction, 80, -80)]
    [InlineData(InvoiceLineItemType.Deduction, -80, -80)]
    [InlineData(InvoiceLineItemType.FuelCard, 50, -50)]
    [InlineData(InvoiceLineItemType.Escrow, 500, 500)]       // release
    [InlineData(InvoiceLineItemType.Reimbursement, 40, 40)]
    public void SignedLineAmount_DeductionsAreNegative(InvoiceLineItemType type, decimal typed, decimal stored)
    {
        Assert.Equal(stored, PayrollInvoice.SignedLineAmount(type, typed));
    }
}
