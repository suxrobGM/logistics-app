using Logistics.Infrastructure.Tax.Data;

namespace Logistics.Infrastructure.Tax.Tests.Data;

public class EUVatRulesTests
{
    [Theory]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("NL")]
    [InlineData("ES")]
    [InlineData("IE")]
    [InlineData("HR")]
    [InlineData("MT")]
    public void IsEUMember_KnownEUCountries_ReturnsTrue(string country)
    {
        Assert.True(EUVatRules.IsEUMember(country));
    }

    [Theory]
    [InlineData("de")]   // case-insensitive
    [InlineData("Fr")]
    public void IsEUMember_IsCaseInsensitive(string country)
    {
        Assert.True(EUVatRules.IsEUMember(country));
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
    public void IsEUMember_NonEUCountries_ReturnsFalse(string? country)
    {
        Assert.False(EUVatRules.IsEUMember(country));
    }

    [Fact]
    public void IsReverseCharge_DifferentEUMembers_WithBuyerVatId_ReturnsTrue()
    {
        Assert.True(EUVatRules.IsReverseCharge("DE", "FR", "FR12345678901"));
    }

    [Fact]
    public void IsReverseCharge_SameCountry_ReturnsFalse()
    {
        Assert.False(EUVatRules.IsReverseCharge("DE", "DE", "DE123456789"));
    }

    [Fact]
    public void IsReverseCharge_NoBuyerVatId_ReturnsFalse()
    {
        // B2C cross-border doesn't trigger reverse charge.
        Assert.False(EUVatRules.IsReverseCharge("DE", "FR", null));
        Assert.False(EUVatRules.IsReverseCharge("DE", "FR", ""));
        Assert.False(EUVatRules.IsReverseCharge("DE", "FR", "  "));
    }

    [Fact]
    public void IsReverseCharge_BuyerOutsideEu_ReturnsFalse()
    {
        // EU seller to non-EU buyer is an export, not reverse charge.
        Assert.False(EUVatRules.IsReverseCharge("DE", "US", "US-EIN-123"));
        Assert.False(EUVatRules.IsReverseCharge("DE", "GB", "GB123456789"));
    }

    [Fact]
    public void IsReverseCharge_SellerOutsideEu_ReturnsFalse()
    {
        Assert.False(EUVatRules.IsReverseCharge("US", "DE", "DE123456789"));
    }

    [Theory]
    [InlineData(null, "FR", "FR123")]
    [InlineData("", "FR", "FR123")]
    [InlineData("DE", null, "FR123")]
    [InlineData("DE", "", "FR123")]
    public void IsReverseCharge_MissingCountry_ReturnsFalse(string? seller, string? buyer, string vatId)
    {
        Assert.False(EUVatRules.IsReverseCharge(seller, buyer, vatId));
    }
}
