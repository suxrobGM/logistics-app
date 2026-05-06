using Logistics.Infrastructure.Tax.Data;

namespace Logistics.Infrastructure.Tax.Tests.Data;

public class RateTableTests
{
    [Theory]
    [InlineData("DE", 19.00)]
    [InlineData("FR", 20.00)]
    [InlineData("NL", 21.00)]
    [InlineData("HU", 27.00)] // highest standard EU VAT
    [InlineData("LU", 17.00)] // lowest standard EU VAT
    [InlineData("GB", 20.00)] // post-Brexit UK
    [InlineData("CH", 8.10)]  // Swiss
    public void EuVatRates_KnownCountries_ReturnExpectedRate(string country, decimal expected)
    {
        Assert.Equal(expected, EuVatRates.GetStandardRate(country));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void EuVatRates_UnknownCountries_ReturnNull(string? country)
    {
        Assert.Null(EuVatRates.GetStandardRate(country));
    }

    [Theory]
    [InlineData("CA", 7.25)]
    [InlineData("TX", 6.25)]
    [InlineData("NY", 4.00)]
    [InlineData("AK", 0.00)] // no statewide sales tax
    [InlineData("DE", 0.00)] // Delaware, not Germany
    [InlineData("DC", 6.00)]
    public void UsSalesTaxRates_KnownStates_ReturnExpectedRate(string state, decimal expected)
    {
        Assert.Equal(expected, UsSalesTaxRates.GetStateBaseRate(state));
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void UsSalesTaxRates_UnknownStates_ReturnNull(string? state)
    {
        Assert.Null(UsSalesTaxRates.GetStateBaseRate(state));
    }

    [Theory]
    [InlineData("AU", 10.00)]
    [InlineData("NZ", 15.00)]
    [InlineData("CA", 5.00)]   // GST only
    [InlineData("JP", 10.00)]
    [InlineData("IN", 18.00)]
    [InlineData("MX", 16.00)]
    public void OtherCountryRates_KnownCountries_ReturnExpectedRate(string country, decimal expected)
    {
        Assert.Equal(expected, OtherCountryRates.GetRate(country));
    }

    [Theory]
    [InlineData("US")]   // not in 'other' table — handled by UsSalesTaxRates
    [InlineData("DE")]   // EU member — handled by EuVatRates
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
        Assert.True(EuVatRates.LastUpdated.Year >= 2026);
        Assert.True(UsSalesTaxRates.LastUpdated.Year >= 2026);
        Assert.True(OtherCountryRates.LastUpdated.Year >= 2026);
    }
}
