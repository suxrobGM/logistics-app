using System.Text.Json;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Ifta;

public class IftaReportServiceTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();

    private readonly ITenantRepository<TruckJurisdictionMileage, Guid> mileageRepo =
        Substitute.For<ITenantRepository<TruckJurisdictionMileage, Guid>>();
    private readonly ITenantRepository<Expense, Guid> expenseRepo =
        Substitute.For<ITenantRepository<Expense, Guid>>();
    private readonly ITenantRepository<IftaQuarterSnapshot, Guid> snapshotRepo =
        Substitute.For<ITenantRepository<IftaQuarterSnapshot, Guid>>();
    private readonly IMasterRepository<IftaTaxRate, Guid> rateRepo =
        Substitute.For<IMasterRepository<IftaTaxRate, Guid>>();

    private readonly IftaReportService sut;
    private readonly Guid truckId = Guid.NewGuid();

    public IftaReportServiceTests()
    {
        tenantUow.Repository<TruckJurisdictionMileage>().Returns(mileageRepo);
        tenantUow.Repository<Expense>().Returns(expenseRepo);
        tenantUow.Repository<IftaQuarterSnapshot>().Returns(snapshotRepo);
        masterUow.Repository<IftaTaxRate>().Returns(rateRepo);

        SetupMileage([]);
        SetupExpenses([]);
        SetupRates([]);
        snapshotRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<IftaQuarterSnapshot, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((IftaQuarterSnapshot?)null);

        sut = new IftaReportService(tenantUow, masterUow);
    }

    private void SetupMileage(List<TruckJurisdictionMileage> rows) =>
        mileageRepo.Query().Returns(rows.BuildMock());

    private void SetupExpenses(List<Expense> rows) =>
        expenseRepo.Query().Returns(rows.BuildMock());

    private void SetupRates(List<IftaTaxRate> rates) =>
        rateRepo.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<IftaTaxRate, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(rates);

    private TruckJurisdictionMileage Mileage(string region, decimal miles, DateOnly? date = null) => new()
    {
        TruckId = truckId,
        Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = region },
        Date = date ?? new DateOnly(2026, 5, 10),
        Miles = miles
    };

    private TruckExpense Fuel(string? region, decimal quantity, VolumeUnit unit = VolumeUnit.Gallons) => new()
    {
        TruckId = truckId,
        Category = TruckExpenseCategory.Fuel,
        Amount = new Money { Amount = quantity * 4, Currency = "USD" },
        ExpenseDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
        Quantity = quantity,
        QuantityUnit = unit,
        PurchaseJurisdiction = region is null ? null : new TaxJurisdiction { CountryCode = "US", Region = region }
    };

    private static IftaTaxRate Rate(string region, decimal rate, decimal? surcharge = null) => new()
    {
        Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = region },
        Year = 2026,
        Quarter = 2,
        RatePerGallon = rate,
        SurchargeRatePerGallon = surcharge
    };

    [Fact]
    public async Task BuildReport_KnownInputs_ComputesTaxDue()
    {
        // 1000 mi TX + 500 mi OK; 200 gal purchased in TX, 100 in OK → MPG = 1500/300 = 5
        SetupMileage([Mileage("TX", 1000), Mileage("OK", 500)]);
        SetupExpenses([Fuel("TX", 200), Fuel("OK", 100)]);
        SetupRates([Rate("TX", 0.20m), Rate("OK", 0.19m)]);

        var report = await sut.BuildReportAsync(2026, 2);

        Assert.Equal(1500m, report.TotalMiles);
        Assert.Equal(300m, report.TotalGallons);
        Assert.Equal(5m, report.AverageMpg);

        var tx = report.Jurisdictions.Single(r => r.Region == "TX");
        Assert.Equal(200m, tx.TaxableGallons);   // 1000 / 5
        Assert.Equal(0m, tx.NetTaxableGallons);  // 200 - 200
        Assert.Equal(0m, tx.TaxDue);

        var ok = report.Jurisdictions.Single(r => r.Region == "OK");
        Assert.Equal(100m, ok.TaxableGallons);   // 500 / 5
        Assert.Equal(0m, ok.NetTaxableGallons);
        Assert.False(report.HasMissingRates);
    }

    [Fact]
    public async Task BuildReport_MoreMilesThanFuel_ProducesPositiveTaxDue()
    {
        // 1000 mi TX, no TX fuel; 100 gal bought in OK, 0 OK miles → MPG = 10
        SetupMileage([Mileage("TX", 1000)]);
        SetupExpenses([Fuel("OK", 100)]);
        SetupRates([Rate("TX", 0.20m), Rate("OK", 0.19m)]);

        var report = await sut.BuildReportAsync(2026, 2);

        var tx = report.Jurisdictions.Single(r => r.Region == "TX");
        Assert.Equal(100m, tx.TaxableGallons);
        Assert.Equal(100m, tx.NetTaxableGallons);
        Assert.Equal(20m, tx.TaxDue); // 100 × 0.20

        var ok = report.Jurisdictions.Single(r => r.Region == "OK");
        Assert.Equal(-100m, ok.NetTaxableGallons); // credit
        Assert.Equal(-19m, ok.TaxDue);
        Assert.Equal(1m, report.TotalTaxDue); // 20 - 19
    }

    [Fact]
    public async Task BuildReport_Surcharge_AppliesToTaxableNotNet()
    {
        // 500 mi IN, 100 gal purchased in IN → MPG 5, taxable 100, net 0
        SetupMileage([Mileage("IN", 500)]);
        SetupExpenses([Fuel("IN", 100)]);
        SetupRates([Rate("IN", 0.57m, surcharge: 0.21m)]);

        var report = await sut.BuildReportAsync(2026, 2);

        var row = report.Jurisdictions.Single(r => r.Region == "IN");
        Assert.Equal(0m, row.NetTaxableGallons);
        // Surcharge is levied on all 100 taxable gallons even though net is zero
        Assert.Equal(21m, row.TaxDue);
    }

    [Fact]
    public async Task BuildReport_MissingRate_FlagsRowWithoutGuessing()
    {
        SetupMileage([Mileage("TX", 500)]);
        SetupExpenses([Fuel("TX", 100)]);
        SetupRates([]);

        var report = await sut.BuildReportAsync(2026, 2);

        var row = report.Jurisdictions.Single();
        Assert.True(row.RateMissing);
        Assert.Null(row.TaxDue);
        Assert.True(report.HasMissingRates);
        Assert.Equal(0m, report.TotalTaxDue);
    }

    [Fact]
    public async Task BuildReport_LiterPurchases_ConvertToGallons()
    {
        SetupMileage([Mileage("TX", 100)]);
        SetupExpenses([Fuel("TX", 378.5411784m, VolumeUnit.Liters)]); // = 100 gallons
        SetupRates([Rate("TX", 0.20m)]);

        var report = await sut.BuildReportAsync(2026, 2);

        Assert.Equal(100m, report.TotalGallons);
    }

    [Fact]
    public async Task GetReport_SnapshotExists_ServedVerbatim()
    {
        var frozen = new IftaReportDto
        {
            Year = 2026,
            Quarter = 1,
            IsClosed = true,
            ClosedAt = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
            TotalMiles = 1234m
        };
        snapshotRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<IftaQuarterSnapshot, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new IftaQuarterSnapshot
            {
                Year = 2026,
                Quarter = 1,
                ClosedAt = frozen.ClosedAt.Value,
                ReportJson = JsonSerializer.Serialize(frozen)
            });

        var report = await sut.GetReportAsync(2026, 1);

        Assert.True(report.IsClosed);
        Assert.Equal(1234m, report.TotalMiles);
    }
}
