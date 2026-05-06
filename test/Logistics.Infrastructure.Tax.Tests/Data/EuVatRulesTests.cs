using Logistics.Infrastructure.Tax.Data;

namespace Logistics.Infrastructure.Tax.Tests.Data;

public class EuVatRulesTests
{
    [Theory]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("NL")]
    [InlineData("ES")]
    [InlineData("IE")]
    [InlineData("HR")]
    [InlineData("MT")]
    public void IsEuMember_KnownEuCountries_ReturnsTrue(string country)
    {
        Assert.True(EuVatRules.IsEuMember(country));
    }

    [Theory]
    [InlineData("de")]   // case-insensitive
    [InlineData("Fr")]
    public void IsEuMember_IsCaseInsensitive(string country)
    {
        Assert.True(EuVatRules.IsEuMember(country));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("CA")]
    [InlineData("GB")]   // UK left the EU
    [InlineData("CH")]   // Switzerland is EFTA, not EU
    [InlineData("NO")]   // Norway is EEA, not EU VAT area for these rules
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void IsEuMember_NonEuCountries_ReturnsFalse(string? country)
    {
        Assert.False(EuVatRules.IsEuMember(country));
    }

    [Fact]
    public void IsReverseCharge_DifferentEuMembers_WithBuyerVatId_ReturnsTrue()
    {
        Assert.True(EuVatRules.IsReverseCharge("DE", "FR", "FR12345678901"));
    }

    [Fact]
    public void IsReverseCharge_SameCountry_ReturnsFalse()
    {
        Assert.False(EuVatRules.IsReverseCharge("DE", "DE", "DE123456789"));
    }

    [Fact]
    public void IsReverseCharge_NoBuyerVatId_ReturnsFalse()
    {
        // B2C cross-border doesn't trigger reverse charge.
        Assert.False(EuVatRules.IsReverseCharge("DE", "FR", null));
        Assert.False(EuVatRules.IsReverseCharge("DE", "FR", ""));
        Assert.False(EuVatRules.IsReverseCharge("DE", "FR", "  "));
    }

    [Fact]
    public void IsReverseCharge_BuyerOutsideEu_ReturnsFalse()
    {
        // EU seller to non-EU buyer is an export, not reverse charge.
        Assert.False(EuVatRules.IsReverseCharge("DE", "US", "US-EIN-123"));
        Assert.False(EuVatRules.IsReverseCharge("DE", "GB", "GB123456789"));
    }

    [Fact]
    public void IsReverseCharge_SellerOutsideEu_ReturnsFalse()
    {
        Assert.False(EuVatRules.IsReverseCharge("US", "DE", "DE123456789"));
    }

    [Theory]
    [InlineData(null, "FR", "FR123")]
    [InlineData("", "FR", "FR123")]
    [InlineData("DE", null, "FR123")]
    [InlineData("DE", "", "FR123")]
    public void IsReverseCharge_MissingCountry_ReturnsFalse(string? seller, string? buyer, string vatId)
    {
        Assert.False(EuVatRules.IsReverseCharge(seller, buyer, vatId));
    }
}
