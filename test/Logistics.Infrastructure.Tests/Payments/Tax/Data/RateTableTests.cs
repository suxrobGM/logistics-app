using Logistics.Infrastructure.Payments.Tax.Data;

namespace Logistics.Infrastructure.Tests.Payments.Tax.Data;

public class RateTableTests
{
    public static TheoryData<string, decimal> EUVatRates_KnownCountries_ReturnExpectedRateCases => new()
    {
        { "DE", 19.00m },
        { "FR", 20.00m },
        { "NL", 21.00m },
        { "HU", 27.00m }, // highest standard EU VAT
        { "LU", 17.00m }, // lowest standard EU VAT
        { "GB", 20.00m }, // post-Brexit UK
        { "CH", 8.10m }, // Swiss
    };

    [Theory]
    [MemberData(nameof(EUVatRates_KnownCountries_ReturnExpectedRateCases))]
    public void EUVatRates_KnownCountries_ReturnExpectedRate(string country, decimal expected)
    {
        Assert.Equal(expected, EUVatRates.GetStandardRate(country));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void EUVatRates_UnknownCountries_ReturnNull(string? country)
    {
        Assert.Null(EUVatRates.GetStandardRate(country));
    }

    public static TheoryData<string, decimal> USSalesTaxRates_KnownStates_ReturnExpectedRateCases => new()
    {
        { "CA", 7.25m },
        { "TX", 6.25m },
        { "NY", 4.00m },
        { "AK", 0.00m }, // no statewide sales tax
        { "DE", 0.00m }, // Delaware, not Germany
        { "DC", 6.00m },
    };

    [Theory]
    [MemberData(nameof(USSalesTaxRates_KnownStates_ReturnExpectedRateCases))]
    public void USSalesTaxRates_KnownStates_ReturnExpectedRate(string state, decimal expected)
    {
        Assert.Equal(expected, USSalesTaxRates.GetStateBaseRate(state));
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void USSalesTaxRates_UnknownStates_ReturnNull(string? state)
    {
        Assert.Null(USSalesTaxRates.GetStateBaseRate(state));
    }

    public static TheoryData<string, decimal> OtherCountryRates_KnownCountries_ReturnExpectedRateCases => new()
    {
        { "AU", 10.00m },
        { "NZ", 15.00m },
        { "CA", 5.00m }, // GST only
        { "JP", 10.00m },
        { "IN", 18.00m },
        { "MX", 16.00m },
    };

    [Theory]
    [MemberData(nameof(OtherCountryRates_KnownCountries_ReturnExpectedRateCases))]
    public void OtherCountryRates_KnownCountries_ReturnExpectedRate(string country, decimal expected)
    {
        Assert.Equal(expected, OtherCountryRates.GetRate(country));
    }

    [Theory]
    [InlineData("US")]   // not in 'other' table - handled by USSalesTaxRates
    [InlineData("DE")]   // EU member - handled by EUVatRates
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void OtherCountryRates_NonOtherCountries_ReturnNull(string? country)
    {
        Assert.Null(OtherCountryRates.GetRate(country));
    }

    [Fact]
    public void RateTables_HaveLastUpdatedDates()
    {
        // Sanity check that the freshness markers are populated.
        Assert.True(EUVatRates.LastUpdated.Year >= 2026);
        Assert.True(USSalesTaxRates.LastUpdated.Year >= 2026);
        Assert.True(OtherCountryRates.LastUpdated.Year >= 2026);
    }
}
