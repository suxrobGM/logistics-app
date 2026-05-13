using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Tax.Manual;
using Microsoft.Extensions.Logging.Abstractions;
using static Logistics.Infrastructure.Tax.Tests.TaxTestBuilders;

namespace Logistics.Infrastructure.Tax.Tests.Manual;

public class ManualTaxCalculatorTests
{
    private readonly ManualTaxCalculator sut;

    public ManualTaxCalculatorTests()
    {
        sut = new ManualTaxCalculator(MasterUowWithRates(), NullLogger<ManualTaxCalculator>.Instance);
    }

    private ManualTaxCalculator WithRates(params Logistics.Domain.Entities.TenantTaxRate[] rates) =>
        new(MasterUowWithRates(rates), NullLogger<ManualTaxCalculator>.Instance);

    #region Short-circuits

    [Fact]
    public async Task Calculate_VatExemptCustomer_ReturnsZeroExclusive()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, exempt: true);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.Exclusive, result.TaxBehavior);
        Assert.All(result.Lines, l => Assert.Equal(0m, l.TaxAmount));
        Assert.Single(result.Breakdown);
        Assert.Equal("Customer is VAT/tax exempt", result.Breakdown[0].Description);
    }

    [Fact]
    public async Task Calculate_EmptyLineItems_ReturnsEmptyBreakdownLines()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, lineAmounts: []);

        var result = await sut.CalculateAsync(request);

        Assert.Empty(result.Lines);
        // ZeroResult still emits a single breakdown row with BaseAmount = 0.
        Assert.Single(result.Breakdown);
        Assert.Equal(0m, result.Breakdown[0].BaseAmount);
    }

    #endregion

    #region Reverse charge (EU B2B)

    [Fact]
    public async Task Calculate_EuCrossBorderB2B_TriggersReverseCharge()
    {
        var request = Request(
            country: "FR",
            tenantRegion: Region.EU,
            customerTaxId: "FR12345678901",
            lineAmounts: [100m, 50m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.ReverseCharge, result.TaxBehavior);
        Assert.All(result.Lines, l =>
        {
            Assert.Equal(0m, l.TaxAmount);
            Assert.Equal(0m, l.RatePercent);
        });
        Assert.Contains("Reverse charge", result.Breakdown[0].Description);
    }

    [Fact]
    public async Task Calculate_EuSameCountry_AppliesStandardVat()
    {
        var request = Request(
            country: "DE",
            tenantRegion: Region.EU,
            customerTaxId: "DE111111111",
            lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.Exclusive, result.TaxBehavior);
        Assert.Equal(19.00m, result.Lines.Single().RatePercent);
        Assert.Equal(19.00m, result.Lines.Single().TaxAmount);
    }

    [Fact]
    public async Task Calculate_EuB2C_NoVatId_NoReverseCharge_AppliesDestinationVat()
    {
        var request = Request(
            country: "FR",
            tenantRegion: Region.EU,
            customerTaxId: null,
            lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.Exclusive, result.TaxBehavior);
        Assert.Equal(20.00m, result.Lines.Single().RatePercent);
    }

    [Fact]
    public async Task Calculate_UsTenantUsCustomer_NeverReverseCharge()
    {
        // Reverse charge is EU-only; same conditions in US should not trigger it.
        var request = Request(
            country: "US",
            state: "CA",
            tenantRegion: Region.US,
            customerTaxId: "12-3456789");

        var result = await sut.CalculateAsync(request);

        Assert.NotEqual(TaxBehavior.ReverseCharge, result.TaxBehavior);
    }

    [Fact]
    public async Task Calculate_UsesTenantTaxResidencyCountry_OverAddressCountry()
    {
        // Tenant address says DE but tax residency is FR.
        var request = Request(
            country: "DE",                    // customer
            tenantRegion: Region.EU,
            customerTaxId: "DE123",
            tenantCountry: "FR",              // residency
            lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.ReverseCharge, result.TaxBehavior);
    }

    #endregion

    #region Rate fallback chain

    [Fact]
    public async Task Calculate_UsTenantToUsState_AppliesStateBaseRate_WithLocalTaxWarning()
    {
        var request = Request(country: "US", state: "CA", tenantRegion: Region.US, lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(7.25m, result.Lines.Single().RatePercent);
        Assert.Equal(7.25m, result.Lines.Single().TaxAmount);
        Assert.NotNull(result.Warning);
        Assert.Contains("local taxes not included", result.Warning);
    }

    [Fact]
    public async Task Calculate_UsTenantToUnknownState_FallsThroughToZero()
    {
        var request = Request(country: "US", state: "ZZ", tenantRegion: Region.US, lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.All(result.Lines, l => Assert.Equal(0m, l.TaxAmount));
        Assert.NotNull(result.Warning);
    }

    [Theory]
    [InlineData("AU", 10.00)]
    [InlineData("NZ", 15.00)]
    [InlineData("CA", 5.00)]
    public async Task Calculate_OtherCountry_AppliesCountryDefault(string country, decimal expectedRate)
    {
        var request = Request(country: country, state: "", tenantRegion: Region.US, lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(expectedRate, result.Lines.Single().RatePercent);
    }

    [Fact]
    public async Task Calculate_NoApplicableRate_ReturnsZeroWithWarning()
    {
        // Antarctica isn't in any table.
        var request = Request(country: "AQ", state: "", tenantRegion: Region.US, lineAmounts: [100m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(0m, result.Lines.Single().RatePercent);
        Assert.Equal(0m, result.Lines.Single().TaxAmount);
        Assert.NotNull(result.Warning);
    }

    #endregion

    #region Tenant-managed rate precedence

    [Fact]
    public async Task Calculate_TenantRateActive_BeatsEuDefault()
    {
        var tenantId = Guid.NewGuid();
        var sutWithRate = WithRates(TenantRate(tenantId, country: "DE", ratePercent: 7.00m, taxCode: "txcd_custom"));

        var request = Request(country: "DE", tenantRegion: Region.EU, tenantId: tenantId, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        Assert.Equal(7.00m, result.Lines.Single().RatePercent);
        Assert.Equal("txcd_custom", result.Lines.Single().TaxCode);
    }

    [Fact]
    public async Task Calculate_StateSpecificRate_BeatsCountryRate()
    {
        var tenantId = Guid.NewGuid();
        var sutWithRate = WithRates(
            TenantRate(tenantId, country: "US", region: null,    ratePercent: 5.00m, description: "country"),
            TenantRate(tenantId, country: "US", region: "CA",   ratePercent: 9.50m, description: "state"));

        var request = Request(country: "US", state: "CA", tenantRegion: Region.US, tenantId: tenantId, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        Assert.Equal(9.50m, result.Lines.Single().RatePercent);
        Assert.Equal("state", result.Breakdown[0].Description);
    }

    [Fact]
    public async Task Calculate_ExpiredRate_IsIgnored()
    {
        var tenantId = Guid.NewGuid();
        var sutWithRate = WithRates(
            TenantRate(tenantId, country: "DE",
                ratePercent: 7.00m,
                from: DateTime.UtcNow.AddDays(-30),
                to: DateTime.UtcNow.AddDays(-1)));

        var request = Request(country: "DE", tenantRegion: Region.EU, tenantId: tenantId, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        // Falls back to EU default 19% rather than the expired 7%.
        Assert.Equal(19.00m, result.Lines.Single().RatePercent);
    }

    [Fact]
    public async Task Calculate_FutureRate_IsIgnored()
    {
        var tenantId = Guid.NewGuid();
        var sutWithRate = WithRates(
            TenantRate(tenantId, country: "DE",
                ratePercent: 7.00m,
                from: DateTime.UtcNow.AddDays(10)));

        var request = Request(country: "DE", tenantRegion: Region.EU, tenantId: tenantId, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        Assert.Equal(19.00m, result.Lines.Single().RatePercent);
    }

    [Fact]
    public async Task Calculate_OtherTenantsRate_IsIgnored()
    {
        var thisTenant = Guid.NewGuid();
        var otherTenant = Guid.NewGuid();
        var sutWithRate = WithRates(TenantRate(otherTenant, "DE", ratePercent: 7.00m));

        var request = Request(country: "DE", tenantRegion: Region.EU, tenantId: thisTenant, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        Assert.Equal(19.00m, result.Lines.Single().RatePercent);
    }

    [Fact]
    public async Task Calculate_TwoCountryRates_PicksMostRecent()
    {
        var tenantId = Guid.NewGuid();
        var sutWithRate = WithRates(
            TenantRate(tenantId, "DE", ratePercent: 16.00m, from: DateTime.UtcNow.AddYears(-1), description: "old"),
            TenantRate(tenantId, "DE", ratePercent: 19.00m, from: DateTime.UtcNow.AddDays(-30), description: "current"));

        var request = Request(country: "DE", tenantRegion: Region.EU, tenantId: tenantId, lineAmounts: [100m]);

        var result = await sutWithRate.CalculateAsync(request);

        Assert.Equal("current", result.Breakdown[0].Description);
    }

    #endregion

    #region Multi-line math

    [Fact]
    public async Task Calculate_MultipleLines_SumsTaxAcrossLines()
    {
        var request = Request(
            country: "DE", tenantRegion: Region.EU,
            lineAmounts: [100m, 50m, 25m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(3, result.Lines.Count);
        Assert.Equal(19.00m, result.Lines.ElementAt(0).TaxAmount);
        Assert.Equal(9.50m, result.Lines.ElementAt(1).TaxAmount);
        Assert.Equal(4.75m, result.Lines.ElementAt(2).TaxAmount);
        Assert.Equal(33.25m, result.Breakdown.Single().TaxAmount);
        Assert.Equal(175.00m, result.Breakdown.Single().BaseAmount);
    }

    [Fact]
    public async Task Calculate_PerLineTaxRoundedToTwoDecimals()
    {
        // 33.33 * 9% = 2.9997 → 3.00
        var request = Request(country: "AU", tenantRegion: Region.US, lineAmounts: [33.33m]);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(3.33m, result.Lines.Single().TaxAmount); // 33.33 * 0.10 = 3.333 → 3.33
    }

    #endregion
}
