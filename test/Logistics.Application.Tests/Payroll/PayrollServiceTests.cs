using System.Linq.Expressions;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class PayrollServiceTests
{
    private static readonly DateTime Start = new(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly StartDay = DateOnly.FromDateTime(Start);
    private static readonly DateOnly EndDay = DateOnly.FromDateTime(End);

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly List<Load> _loads = [];
    private readonly List<Truck> _trucks = [];
    private readonly List<RecurringDeduction> _deductions = [];
    private readonly List<CashAdvance> _advances = [];
    private readonly List<FuelCardTransaction> _fuel = [];
    private readonly List<PayrollInvoice> _payrolls = [];
    private readonly List<TripStop> _stops = [];
    private readonly Employee _driver = new() { Email = "d@x.com", FirstName = "Ana", LastName = "Diaz", Salary = Money.Zero("USD") };
    private readonly Truck _truck;
    private readonly PayrollService _sut;

    public PayrollServiceTests()
    {
        _truck = new Truck { Number = "T-1", Type = TruckType.FreightTruck, MainDriverId = _driver.Id, MainDriver = _driver };
        _trucks.Add(_truck);

        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        Wire(_loads);
        Wire(_trucks);
        Wire(_deductions);
        Wire(_advances);
        Wire(_fuel);
        Wire(_payrolls);
        Wire(_stops);
        Wire(new List<TimeEntry>());
        _tenantUow.Repository<InvoiceLineItem>().Returns(Substitute.For<ITenantRepository<InvoiceLineItem, Guid>>());

        _sut = new PayrollService(Substitute.For<IMasterUnitOfWork>(), _tenantUow, NullLogger<PayrollService>.Instance);
    }

    private void Wire<T>(List<T> items) where T : class, IEntity<Guid>, ITenantEntity
    {
        var repo = Substitute.For<ITenantRepository<T, Guid>>();
        repo.Query().Returns(_ => items.BuildMock());
        repo.GetListAsync(Arg.Any<Expression<Func<T, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => items.Where(ci.Arg<Expression<Func<T, bool>>>().Compile()).ToList());
        _tenantUow.Repository<T>().Returns(repo);
    }

    private Load Delivered(decimal linehaul, decimal fuelSurcharge = 0, decimal accessorials = 0)
    {
        var load = LoadInvoiceChargesTests.NewLoad(linehaul);
        load.FuelSurcharge = fuelSurcharge;
        if (accessorials > 0)
        {
            load.Accessorials.Add(new LoadAccessorial
            {
                Load = load, Name = "Detention", LineItemType = InvoiceLineItemType.Detention,
                Unit = AccessorialUnit.Flat, Rate = accessorials
            });
        }

        load.AssignToTruck(_truck);
        load.UpdateStatus(LoadStatus.Delivered, force: true);
        typeof(Load).GetProperty(nameof(Load.DeliveredAt))!.SetValue(load, Start.AddDays(2));
        _loads.Add(load);
        return load;
    }

    #region Gross pay

    [Theory]
    [InlineData(false, false, 0.88 * 2600)]
    [InlineData(true, false, 0.88 * 2200)]
    [InlineData(false, true, 0.88 * 2400)]
    [InlineData(true, true, 0.88 * 2000)]
    public async Task Build_ShareOfGross_AppliesExclusions(bool excludeFuel, bool excludeAccessorials, double expected)
    {
        _driver.SalaryType = SalaryType.ShareOfGross;
        _driver.Salary = new Money { Amount = 0.88m, Currency = "USD" };
        _driver.PayExcludesFuelSurcharge = excludeFuel;
        _driver.PayExcludesAccessorials = excludeAccessorials;
        Delivered(2000m, fuelSurcharge: 400m, accessorials: 200m);

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal((decimal)expected, payroll.LineItems.Single(l => l.Type == InvoiceLineItemType.BasePay).Total);
    }

    /// <summary>Load distances are meters; paying the rate on them raw overpaid by ~1600x.</summary>
    [Theory]
    [InlineData(DistanceUnit.Miles, 160_934.4, 0.65, 65.00)]
    [InlineData(DistanceUnit.Kilometers, 100_000, 0.50, 50.00)]
    public async Task Build_RatePerDistance_PaysPerTenantUnit(
        DistanceUnit unit, double meters, double rate, double expected)
    {
        var tenant = TestTenant.Create();
        tenant.Settings.DistanceUnit = unit;
        _tenantUow.GetCurrentTenant().Returns(tenant);
        _driver.SalaryType = SalaryType.RatePerDistance;
        _driver.Salary = new Money { Amount = (decimal)rate, Currency = "USD" };
        Delivered(1000m).Distance = meters;

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal((decimal)expected, Math.Round(payroll.Total.Amount, 2));
        Assert.Equal(meters, payroll.TotalDistanceDriven);
    }

    [Fact]
    public async Task Build_PerLoad_PaysFlatPerDeliveredLoad()
    {
        _driver.SalaryType = SalaryType.PerLoad;
        _driver.Salary = new Money { Amount = 250m, Currency = "USD" };
        Delivered(1000m);
        Delivered(3000m);
        var outsidePeriod = Delivered(500m);
        typeof(Load).GetProperty(nameof(Load.DeliveredAt))!.SetValue(outsidePeriod, End.AddDays(3));

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal(500m, payroll.Total.Amount);
    }

    [Fact]
    public async Task Build_LoadDeliveredLateOnTheLastDay_IsInThePeriod()
    {
        _driver.SalaryType = SalaryType.PerLoad;
        _driver.Salary = new Money { Amount = 250m, Currency = "USD" };
        var lastDay = Delivered(1000m);
        typeof(Load).GetProperty(nameof(Load.DeliveredAt))!.SetValue(lastDay, End.AddHours(23).AddMinutes(59));
        var nextDay = Delivered(1000m);
        typeof(Load).GetProperty(nameof(Load.DeliveredAt))!.SetValue(nextDay, End.AddDays(1));

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal(250m, payroll.Total.Amount);
        Assert.Equal(StartDay, payroll.PeriodStart);
        Assert.Equal(EndDay, payroll.PeriodEnd);
    }

    [Fact]
    public async Task Build_PerStop_CountsStopsDepartedInPeriod()
    {
        _driver.SalaryType = SalaryType.PerStop;
        _driver.Salary = new Money { Amount = 40m, Currency = "USD" };
        var trip = Trip.Create("Trip", _truck, [LoadInvoiceChargesTests.NewLoad(), LoadInvoiceChargesTests.NewLoad()]);
        foreach (var stop in trip.Stops)
        {
            stop.DepartedAt = Start.AddDays(1);
        }

        trip.Stops[0].DepartedAt = null;
        _stops.AddRange(trip.Stops);

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal(3 * 40m, payroll.Total.Amount);
    }

    #endregion

    #region Deductions

    [Fact]
    public async Task Create_OwnerOperator_DeductsRecurringAdvancesAndFuelAndClaimsThem()
    {
        _driver.SalaryType = SalaryType.ShareOfGross;
        _driver.Salary = new Money { Amount = 0.9m, Currency = "USD" };
        _driver.DeductFuelCardCharges = true;
        Delivered(3000m);
        _deductions.Add(new RecurringDeduction
        {
            EmployeeId = _driver.Id, Type = PayDeductionType.TruckLease, Description = "Truck lease", Amount = 500m
        });
        var advance = new CashAdvance
        {
            EmployeeId = _driver.Id, Amount = new Money { Amount = 300m, Currency = "USD" }
        };
        var earlierInvoiceId = Guid.NewGuid();
        var alreadyDeducted = new CashAdvance
        {
            EmployeeId = _driver.Id, Amount = new Money { Amount = 999m, Currency = "USD" },
            PayrollInvoiceId = earlierInvoiceId
        };
        _advances.AddRange([advance, alreadyDeducted]);
        var fuel = FuelOn(_truck, Start.AddDays(1), 400m);
        var otherTruckFuel = FuelOn(new Truck { Number = "T-2", Type = TruckType.FreightTruck }, Start.AddDays(1), 77m);

        var payroll = await _sut.CreatePayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal(2700m - 500m - 300m - 400m, payroll.Total.Amount);
        Assert.Equal(payroll.Id, advance.PayrollInvoiceId);
        Assert.Equal(payroll.Id, fuel.PayrollInvoiceId);
        Assert.Equal(earlierInvoiceId, alreadyDeducted.PayrollInvoiceId);
        Assert.Null(otherTruckFuel.PayrollInvoiceId);
    }

    [Fact]
    public async Task Build_Preview_DoesNotClaimAdvances()
    {
        var advance = new CashAdvance { EmployeeId = _driver.Id, Amount = new Money { Amount = 100m, Currency = "USD" } };
        _advances.Add(advance);

        var payroll = await _sut.BuildPayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Contains(payroll.LineItems, l => l.Type == InvoiceLineItemType.CashAdvance);
        Assert.Null(advance.PayrollInvoiceId);
    }

    [Fact]
    public async Task Create_FuelNotDeductedUnlessEnabled()
    {
        var fuel = FuelOn(_truck, Start.AddDays(1), 400m);

        var payroll = await _sut.CreatePayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.DoesNotContain(payroll.LineItems, l => l.Type == InvoiceLineItemType.FuelCard);
        Assert.Null(fuel.PayrollInvoiceId);
    }

    [Fact]
    public async Task Create_Escrow_UsesBalanceFromEarlierSettlementsToCapContribution()
    {
        _deductions.Add(new RecurringDeduction
        {
            EmployeeId = _driver.Id, Type = PayDeductionType.Escrow, Description = "Escrow", Amount = 250m,
            EscrowTarget = 1000m
        });
        _payrolls.Add(EarlierSettlement(-400m, InvoiceStatus.Paid));
        _payrolls.Add(EarlierSettlement(-500m, InvoiceStatus.Approved));
        _payrolls.Add(EarlierSettlement(-500m, InvoiceStatus.Rejected));

        var balance = await _sut.GetEscrowBalanceAsync(_driver.Id);
        var payroll = await _sut.CreatePayrollInvoiceAsync(_driver, StartDay, EndDay);

        Assert.Equal(900m, balance);
        Assert.Equal(-100m, payroll.LineItems.Single(l => l.Type == InvoiceLineItemType.Escrow).Total);
    }

    [Fact]
    public async Task Recalculate_ReleasesAndReclaimsAdvances()
    {
        var advance = new CashAdvance { EmployeeId = _driver.Id, Amount = new Money { Amount = 100m, Currency = "USD" } };
        _advances.Add(advance);
        var payroll = await _sut.CreatePayrollInvoiceAsync(_driver, StartDay, EndDay);

        await _sut.RecalculatePayrollAsync(payroll);

        Assert.Equal(payroll.Id, advance.PayrollInvoiceId);
        Assert.Single(payroll.LineItems, l => l.Type == InvoiceLineItemType.CashAdvance);
    }

    #endregion

    private FuelCardTransaction FuelOn(Truck truck, DateTime date, decimal amount)
    {
        var transaction = new FuelCardTransaction
        {
            ProviderType = FuelCardProviderType.Wex,
            ExternalTransactionId = Guid.NewGuid().ToString(),
            TransactionDate = date,
            Amount = new Money { Amount = amount, Currency = "USD" },
            Status = FuelCardTransactionStatus.Matched,
            TruckId = truck.Id
        };
        _fuel.Add(transaction);
        return transaction;
    }

    private PayrollInvoice EarlierSettlement(decimal escrowLine, InvoiceStatus status)
    {
        var payroll = new PayrollInvoice
        {
            EmployeeId = _driver.Id,
            Status = status,
            Subtotal = Money.Zero("USD"),
            TaxTotal = Money.Zero("USD"),
            Total = Money.Zero("USD")
        };
        payroll.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = payroll.Id, Description = "Escrow", Type = InvoiceLineItemType.Escrow,
            Amount = new Money { Amount = escrowLine, Currency = "USD" }
        });
        return payroll;
    }
}
