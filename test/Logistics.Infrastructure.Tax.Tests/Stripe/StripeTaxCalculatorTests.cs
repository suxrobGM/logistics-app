using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Tax.Stripe;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Stripe;
using Stripe.Tax;
using Logistics.Application.Abstractions.Tax;
using static Logistics.Infrastructure.Tax.Tests.TaxTestBuilders;

namespace Logistics.Infrastructure.Tax.Tests.Stripe;

public class StripeTaxCalculatorTests
{
    private readonly IStripeTaxCalculationApi api = Substitute.For<IStripeTaxCalculationApi>();
    private readonly IStripeTaxConfigService config = Substitute.For<IStripeTaxConfigService>();
    private readonly StripeTaxCalculator sut;

    public StripeTaxCalculatorTests()
    {
        config.GetDefaultTaxCodeAsync(Arg.Any<CancellationToken>()).Returns("txcd_99999999");
        sut = new StripeTaxCalculator(api, config, NullLogger<StripeTaxCalculator>.Instance);
    }

    #region Short-circuits

    [Fact]
    public async Task Calculate_VatExempt_ShortCircuits_WithoutCallingStripe()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, exempt: true);

        var result = await sut.CalculateAsync(request);

        Assert.Empty(result.Lines);
        Assert.Empty(result.Breakdown);
        Assert.Equal(TaxBehavior.Exclusive, result.TaxBehavior);
        await api.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Calculate_NoLineItems_ShortCircuits_WithoutCallingStripe()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, lineAmounts: []);

        var result = await sut.CalculateAsync(request);

        Assert.Empty(result.Lines);
        await api.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    #endregion

    #region Request shape

    [Fact]
    public async Task Calculate_BuildsExpectedStripeRequest()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, lineAmounts: [100m, 50.50m],
            customerTaxId: "DE123456789");

        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns(BuildCalculation(request));

        await sut.CalculateAsync(request);

        var captured = (CalculationCreateOptions)api.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(IStripeTaxCalculationApi.CreateAsync))
            .GetArguments()[0]!;

        Assert.Equal("usd", captured.Currency);
        Assert.Equal(2, captured.LineItems.Count);
        Assert.Equal(10000, captured.LineItems[0].Amount);
        Assert.Equal(5050, captured.LineItems[1].Amount);
        Assert.All(captured.LineItems, li => Assert.Equal("txcd_99999999", li.TaxCode));
        Assert.NotNull(captured.CustomerDetails!.TaxIds);
        Assert.Single(captured.CustomerDetails.TaxIds);
        Assert.Equal("eu_vat", captured.CustomerDetails.TaxIds[0].Type);
        Assert.Equal("DE123456789", captured.CustomerDetails.TaxIds[0].Value);
        Assert.Equal("DE", captured.CustomerDetails.Address.Country);
    }

    [Fact]
    public async Task Calculate_NoCustomerTaxId_OmitsTaxIdsArray()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, customerTaxId: null);
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns(BuildCalculation(request));

        await sut.CalculateAsync(request);

        var captured = (CalculationCreateOptions)api.ReceivedCalls().First().GetArguments()[0]!;
        Assert.Null(captured.CustomerDetails!.TaxIds);
    }

    [Fact]
    public async Task Calculate_LineItemTaxCode_OverridesDefault()
    {
        var lineId = Guid.NewGuid();
        var request = new TaxCalculationRequest
        {
            Currency = "USD",
            TenantId = Guid.NewGuid(),
            TenantRegion = Region.US,
            TenantAddress = Address(country: "US", state: "TX"),
            CustomerAddress = Address(country: "US", state: "CA"),
            LineItems = [new TaxCalculationLineItem
            {
                LineItemId = lineId, NetAmount = 100m, TaxCode = "txcd_specific"
            }]
        };

        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns(BuildCalculation(request));

        await sut.CalculateAsync(request);

        var captured = (CalculationCreateOptions)api.ReceivedCalls().First().GetArguments()[0]!;
        Assert.Equal("txcd_specific", captured.LineItems[0].TaxCode);
    }

    [Fact]
    public async Task Calculate_JpyZeroDecimal_DoesNotMultiplyBy100()
    {
        var request = Request(country: "JP", tenantRegion: Region.US, currency: "JPY", lineAmounts: [1000m]);
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns(BuildCalculation(request));

        await sut.CalculateAsync(request);

        var captured = (CalculationCreateOptions)api.ReceivedCalls().First().GetArguments()[0]!;
        Assert.Equal(1000, captured.LineItems[0].Amount);
    }

    #endregion

    #region Response mapping

    [Fact]
    public async Task Calculate_MapsPerLineTaxByReference()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, lineAmounts: [100m, 50m]);
        var lineIds = request.LineItems.Select(l => l.LineItemId).ToArray();

        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data =
                [
                    LineItem(lineIds[0], amountTax: 1900, percentage: "19.00", reason: null),
                    LineItem(lineIds[1], amountTax: 950,  percentage: "19.00", reason: null)
                ]
            }
        };

        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(2, result.Lines.Count);
        var byId = result.Lines.ToDictionary(l => l.LineItemId);
        Assert.Equal(19m, byId[lineIds[0]].TaxAmount);
        Assert.Equal(9.50m, byId[lineIds[1]].TaxAmount);
        Assert.All(result.Lines, l => Assert.Equal(19.00m, l.RatePercent));
    }

    [Fact]
    public async Task Calculate_ReverseChargeReason_SetsBehavior()
    {
        var request = Request(country: "FR", tenantRegion: Region.EU, customerTaxId: "FR12345678901");
        var lineId = request.LineItems.Single().LineItemId;

        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data = [LineItem(lineId, amountTax: 0, percentage: "0", reason: "reverse_charge")]
            }
        };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(TaxBehavior.ReverseCharge, result.TaxBehavior);
        // Synthetic breakdown line surfaces the reverse-charge notice.
        Assert.Single(result.Breakdown);
        Assert.Contains("Reverse charge", result.Breakdown[0].Description);
    }

    [Fact]
    public async Task Calculate_NotCollectingReason_SurfacesWarning()
    {
        var request = Request(country: "TX", state: "TX", tenantRegion: Region.US);
        var lineId = request.LineItems.Single().LineItemId;

        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data = [LineItem(lineId, amountTax: 0, percentage: "0", reason: "not_collecting")]
            }
        };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        Assert.NotNull(result.Warning);
        Assert.Contains("not registered", result.Warning);
    }

    [Fact]
    public async Task Calculate_LineMissingFromResponse_DefensivelyZeroFilled()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU, lineAmounts: [100m, 50m]);
        var lineIds = request.LineItems.Select(l => l.LineItemId).ToArray();

        // Stripe only echoes back the first line.
        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data = [LineItem(lineIds[0], amountTax: 1900, percentage: "19.00", reason: null)]
            }
        };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(2, result.Lines.Count);
        var missing = result.Lines.Single(l => l.LineItemId == lineIds[1]);
        Assert.Equal(0m, missing.TaxAmount);
        Assert.Equal(0m, missing.RatePercent);
    }

    [Fact]
    public async Task Calculate_TopLevelBreakdown_MappedToInvoiceTaxLines()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU);

        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data = [LineItem(request.LineItems[0].LineItemId, 1900, "19.00", null)]
            },
            TaxBreakdown =
            [
                new CalculationTaxBreakdown
                {
                    Amount = 1900,
                    TaxableAmount = 10000,
                    TaxRateDetails = new CalculationTaxBreakdownTaxRateDetails
                    {
                        Country = "DE",
                        State = null,
                        TaxType = "vat",
                        PercentageDecimal = "19.00"
                    }
                }
            ]
        };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        var line = Assert.Single(result.Breakdown);
        Assert.Equal(19m, line.RatePercent);
        Assert.Equal(100m, line.BaseAmount);
        Assert.Equal(19m, line.TaxAmount);
        Assert.Equal("DE", line.Jurisdiction.CountryCode);
        Assert.Null(line.Jurisdiction.Region);
        Assert.Contains("VAT 19% — DE", line.Description);
    }

    [Fact]
    public async Task Calculate_LayeredUsBreakdown_PreservesEachJurisdiction()
    {
        var request = Request(country: "US", state: "CA", tenantRegion: Region.US);

        var calc = new Calculation
        {
            LineItems = new StripeList<CalculationLineItem>
            {
                Data = [LineItem(request.LineItems[0].LineItemId, 850, "8.50", null)]
            },
            TaxBreakdown =
            [
                BreakdownLine("US", "CA", "sales_tax", "6.00", taxable: 10000, amount: 600),
                BreakdownLine("US", "CA", "sales_tax", "1.00", taxable: 10000, amount: 100),
                BreakdownLine("US", "CA", "sales_tax", "1.50", taxable: 10000, amount: 150)
            ]
        };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>()).Returns(calc);

        var result = await sut.CalculateAsync(request);

        Assert.Equal(3, result.Breakdown.Count);
        Assert.All(result.Breakdown, b =>
        {
            Assert.Equal("US", b.Jurisdiction.CountryCode);
            Assert.Equal("CA", b.Jurisdiction.Region);
        });
        Assert.Equal(8.50m, result.Breakdown.Sum(b => b.TaxAmount));
    }

    #endregion

    #region Error handling

    [Fact]
    public async Task Calculate_StripeExceptionWithCode_ReturnsEmptyWithWarning()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU);

        var stripeError = new StripeError { Code = "tax_calculation_error" };
        var ex = new StripeException("boom") { StripeError = stripeError };
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns<Task<Calculation>>(_ => throw ex);

        var result = await sut.CalculateAsync(request);

        Assert.Empty(result.Lines);
        Assert.Empty(result.Breakdown);
        Assert.Contains("tax_calculation_error", result.Warning);
    }

    [Fact]
    public async Task Calculate_StripeExceptionWithoutCode_FallsBackToMessage()
    {
        var request = Request(country: "DE", tenantRegion: Region.EU);
        var ex = new StripeException("network down");
        api.CreateAsync(Arg.Any<CalculationCreateOptions>(), Arg.Any<CancellationToken>())
            .Returns<Task<Calculation>>(_ => throw ex);

        var result = await sut.CalculateAsync(request);

        Assert.NotNull(result.Warning);
        Assert.Contains("network down", result.Warning);
    }

    #endregion

    #region Helpers

    private static Calculation BuildCalculation(TaxCalculationRequest request) => new()
    {
        LineItems = new StripeList<CalculationLineItem>
        {
            Data = request.LineItems
                .Select(li => LineItem(li.LineItemId, amountTax: 0, percentage: "0", reason: null))
                .ToList()
        }
    };

    private static CalculationLineItem LineItem(Guid id, long amountTax, string percentage, string? reason) => new()
    {
        Reference = id.ToString(),
        AmountTax = amountTax,
        TaxBreakdown =
        [
            new CalculationLineItemTaxBreakdown
            {
                Amount = amountTax,
                TaxabilityReason = reason,
                TaxRateDetails = new CalculationLineItemTaxBreakdownTaxRateDetails
                {
                    PercentageDecimal = percentage,
                    TaxType = "vat"
                }
            }
        ]
    };

    private static CalculationTaxBreakdown BreakdownLine(
        string country, string? state, string taxType, string percentage, long taxable, long amount) => new()
    {
        Amount = amount,
        TaxableAmount = taxable,
        TaxRateDetails = new CalculationTaxBreakdownTaxRateDetails
        {
            Country = country,
            State = state,
            TaxType = taxType,
            PercentageDecimal = percentage
        }
    };

    #endregion
}
