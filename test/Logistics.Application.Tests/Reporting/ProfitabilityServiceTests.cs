using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Modules.Reporting.Reports.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Reporting;

public class ProfitabilityServiceTests
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
    private const double Km = 1000;

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly List<Load> _loads = [];
    private readonly List<Truck> _trucks = [];
    private readonly List<Employee> _employees = [];
    private readonly List<Expense> _expenses = [];
    private readonly List<FactoringBatchItem> _factoringItems = [];
    private readonly List<FactoringCompany> _factors = [];
    private readonly List<FuelCardTransaction> _fuel = [];
    private readonly List<TruckJurisdictionDistance> _mileage = [];
    private readonly Customer _customer = new() { Name = "Acme" };
    private readonly Employee _driver = TestData.Driver();
    private readonly Truck _truck;
    private readonly ProfitabilityService _sut;

    public ProfitabilityServiceTests()
    {
        _truck = TestData.Truck(_driver);
        _trucks.Add(_truck);
        _employees.Add(_driver);

        var tenant = TestTenant.Create();
        tenant.Settings.DistanceUnit = DistanceUnit.Kilometers;
        _tenantUow.GetCurrentTenant().Returns(tenant);
        _tenantUow.Stub(_loads);
        _tenantUow.Stub(_trucks);
        _tenantUow.Stub(_employees);
        _tenantUow.Stub(_expenses);
        _tenantUow.Stub(_factoringItems);
        _tenantUow.Stub(_factors);
        _tenantUow.Stub(_fuel);
        _tenantUow.Stub<TripStop>();
        _tenantUow.Stub<TimeEntry>();
        _tenantUow.Stub(_mileage);

        _sut = new ProfitabilityService(_tenantUow);
    }

    private Load Delivered(decimal linehaul, double distance, Customer? customer = null, Truck? truck = null)
    {
        var load = TestData.Load(deliveryCost: linehaul, customer: customer ?? _customer);
        load.Distance = distance;
        load.AssignToTruck(truck ?? _truck);
        typeof(Load).GetProperty(nameof(Load.DeliveredAt))!.SetValue(load, From.AddDays(5));
        _loads.Add(load);
        return load;
    }

    private void TruckCost(decimal amount, TruckExpenseCategory category = TruckExpenseCategory.Fuel,
        ExpenseStatus status = ExpenseStatus.Approved, Truck? truck = null) =>
        _expenses.Add(new TruckExpense
        {
            TruckId = (truck ?? _truck).Id,
            Category = category,
            Status = status,
            Amount = amount,
            ExpenseDate = DateOnly.FromDateTime(From.AddDays(3))
        });

    private void PayDriver(SalaryType type, decimal rate)
    {
        _driver.SalaryType = type;
        _driver.Salary = rate;
    }

    [Fact]
    public void Spread_RoundsToTheCent_AndTheSharesAddUp()
    {
        var shares = ProfitabilityService.Spread(100m, [1, 1, 1]);

        Assert.Equal([33.33m, 33.33m, 33.34m], shares);
    }

    [Fact]
    public void Spread_NoDistance_SplitsEqually()
    {
        var shares = ProfitabilityService.Spread(10m, [0, 0]);

        Assert.Equal([5m, 5m], shares);
    }

    [Fact]
    public async Task Build_TruckCosts_SpreadByLoadedDistance()
    {
        var shortLoad = Delivered(1000m, 100 * Km);
        var longLoad = Delivered(3000m, 300 * Km);
        TruckCost(400m);

        var result = await _sut.BuildAsync(From, To);

        Assert.Equal(100m, result.Loads.Single(l => l.LoadId == shortLoad.Id).TruckCostsTotal);
        Assert.Equal(300m, result.Loads.Single(l => l.LoadId == longLoad.Id).TruckCostsTotal);
    }

    [Fact]
    public async Task Build_PendingAndRejectedExpenses_AreNotCosts()
    {
        var load = Delivered(1000m, 100 * Km);
        TruckCost(50m, status: ExpenseStatus.Paid);
        TruckCost(900m, status: ExpenseStatus.Rejected);
        TruckCost(800m, status: ExpenseStatus.Pending);

        var result = await _sut.BuildAsync(From, To);

        Assert.Equal(50m, result.Loads.Single(l => l.LoadId == load.Id).TruckCostsTotal);
    }

    [Fact]
    public async Task Build_MatchedFuelCardTransaction_CountsOnceThroughItsExpense()
    {
        var load = Delivered(1000m, 100 * Km);
        TruckCost(200m, status: ExpenseStatus.Paid);
        _fuel.Add(FuelTransaction(200m, FuelCardTransactionStatus.Matched, _truck.Id));
        _fuel.Add(FuelTransaction(75m, FuelCardTransactionStatus.Pending, null));

        var result = await _sut.BuildAsync(From, To);

        var row = result.Loads.Single(l => l.LoadId == load.Id);
        Assert.Equal(200m, row.TruckCostsTotal);
        Assert.Equal(75m, result.Summary.UnassignedFuel);
        Assert.Equal(800m, result.Summary.NetProfit);
    }

    [Fact]
    public async Task Build_FactoringFee_FundedThenEstimatedThenNone()
    {
        var factor = new FactoringCompany { Name = "Factor", SubmissionEmail = "f@x.com", FeePercent = 3m, AdvanceRatePercent = 90m };
        _factors.Add(factor);
        var factoredCustomer = new Customer { Name = "Factored", FactoringCompanyId = factor.Id };

        var funded = Delivered(1000m, 100 * Km, factoredCustomer);
        _factoringItems.Add(new FactoringBatchItem
        {
            BatchId = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            Invoice = LoadInvoice.Create(funded),
            Amount = funded.DeliveryCost,
            FeeAmount = 25m,
            FundedAt = From.AddDays(6)
        });
        var estimated = Delivered(2000m, 100 * Km, factoredCustomer);
        var direct = Delivered(2000m, 100 * Km);

        var rows = (await _sut.BuildAsync(From, To)).Loads;

        var fundedRow = rows.Single(l => l.LoadId == funded.Id);
        var estimatedRow = rows.Single(l => l.LoadId == estimated.Id);
        var directRow = rows.Single(l => l.LoadId == direct.Id);
        Assert.Equal((25m, false), (fundedRow.FactoringFee, fundedRow.FactoringFeeEstimated));
        Assert.Equal((60m, true), (estimatedRow.FactoringFee, estimatedRow.FactoringFeeEstimated));
        Assert.Equal((0m, false), (directRow.FactoringFee, directRow.FactoringFeeEstimated));
    }

    [Fact]
    public async Task Build_LoadOnNoTrip_PaysTheTrucksCurrentDriver()
    {
        PayDriver(SalaryType.PerLoad, 150m);
        var load = Delivered(1000m, 100 * Km);

        var row = (await _sut.BuildAsync(From, To)).Loads.Single(l => l.LoadId == load.Id);

        Assert.Equal(150m, row.DriverPay);
        Assert.False(row.DriverPayEstimated);
        Assert.Equal(850m, row.Margin);
        Assert.Equal(8.50m, row.MarginPerDistance);
    }

    [Fact]
    public async Task Build_WeeklySalary_SpreadOverTheDriversLoads()
    {
        PayDriver(SalaryType.Weekly, 700m);
        var first = Delivered(1000m, 100 * Km);
        var second = Delivered(1000m, 100 * Km);

        var rows = (await _sut.BuildAsync(From, From.AddDays(7))).Loads;

        Assert.All(rows, r => Assert.True(r.DriverPayEstimated));
        Assert.Equal(350m, rows.Single(l => l.LoadId == first.Id).DriverPay);
        Assert.Equal(350m, rows.Single(l => l.LoadId == second.Id).DriverPay);
    }

    [Fact]
    public async Task Build_CostsNoLoadAbsorbs_ReconcileToNetProfit()
    {
        PayDriver(SalaryType.PerLoad, 100m);
        Delivered(1000m, 100 * Km);
        Delivered(2000m, 200 * Km);
        TruckCost(300m);

        var idleDriver = TestData.Driver();
        idleDriver.SalaryType = SalaryType.Weekly;
        idleDriver.Salary = 500m;
        _employees.Add(idleDriver);
        var idleTruck = TestData.Truck(idleDriver, number: "T-2");
        _trucks.Add(idleTruck);
        TruckCost(120m, TruckExpenseCategory.Tires, truck: idleTruck);
        _expenses.Add(new CompanyExpense
        {
            Category = CompanyExpenseCategory.Insurance,
            Status = ExpenseStatus.Approved,
            Amount = 400m,
            ExpenseDate = DateOnly.FromDateTime(From.AddDays(1))
        });

        var summary = (await _sut.BuildAsync(From, From.AddDays(7))).Summary;

        Assert.Equal(3000m - 200m - 300m, summary.LoadMargin);
        Assert.Equal(120m, summary.UnallocatedTruckCosts);
        Assert.Equal(500m, summary.UnallocatedDriverPay);
        Assert.Equal(400m, summary.Overhead);
        Assert.Equal(summary.LoadMargin - 120m - 500m - 400m, summary.NetProfit);
    }

    [Fact]
    public async Task Build_SalariedStaffWhoDoNotDrive_CountedAsStaffPay()
    {
        Delivered(1000m, 100 * Km);
        var dispatcher = TestData.Driver();
        dispatcher.SalaryType = SalaryType.Monthly;
        dispatcher.Salary = 4000m;
        var formerDispatcher = TestData.Driver();
        formerDispatcher.SalaryType = SalaryType.Monthly;
        formerDispatcher.Salary = 9000m;
        formerDispatcher.Status = EmployeeStatus.Terminated;
        _employees.AddRange([dispatcher, formerDispatcher]);

        var summary = (await _sut.BuildAsync(From, From.AddDays(7))).Summary;

        Assert.Equal(Math.Round(4000m * 7 / (365.25m / 12), 2), summary.StaffPay);
        Assert.Equal(summary.LoadMargin - summary.StaffPay, summary.NetProfit);
    }

    [Fact]
    public async Task Build_MileageLogShorterThanLoadedDistance_TreatedAsMissing()
    {
        Delivered(1000m, 500 * Km);
        _mileage.Add(Mileage(_truck, miles: 100m));

        var truck = (await _sut.BuildAsync(From, To)).Trucks.Single();

        Assert.Null(truck.ActualDistance);
        Assert.Null(truck.DeadheadPercent);
    }

    [Fact]
    public async Task Build_MileageLog_GivesDeadheadShare()
    {
        Delivered(1000m, DistanceUnit.Miles.ToMeters(300));
        _mileage.Add(Mileage(_truck, miles: 400m));

        var truck = (await _sut.BuildAsync(From, To)).Trucks.Single();

        Assert.Equal(25, truck.DeadheadPercent);
    }

    [Fact]
    public async Task Build_Trucks_GroupLoadsAndKeepIdleTrucksCosts()
    {
        Delivered(1000m, 100 * Km);
        TruckCost(100m);
        var idleTruck = TestData.Truck(number: "T-2");
        _trucks.Add(idleTruck);
        TruckCost(80m, TruckExpenseCategory.Tires, truck: idleTruck);

        var trucks = (await _sut.BuildAsync(From, To)).Trucks;

        Assert.Equal(-80m, trucks.Single(t => t.TruckId == idleTruck.Id).Margin);
        var busy = trucks.Single(t => t.TruckId == _truck.Id);
        Assert.Equal((1, 900m), (busy.LoadCount, busy.Margin));
    }

    private static TruckJurisdictionDistance Mileage(Truck truck, decimal miles) => new()
    {
        TruckId = truck.Id,
        Jurisdiction = TaxJurisdiction.Create("US", "TX"),
        Date = DateOnly.FromDateTime(From.AddDays(2)),
        Meters = miles * 1609.344m
    };

    private FuelCardTransaction FuelTransaction(decimal amount, FuelCardTransactionStatus status, Guid? truckId) => new()
    {
        ProviderType = FuelCardProviderType.Wex,
        ExternalTransactionId = Guid.NewGuid().ToString(),
        TransactionDate = From.AddDays(3),
        Amount = new Money { Amount = amount, Currency = "USD" },
        Status = status,
        TruckId = truckId
    };
}
