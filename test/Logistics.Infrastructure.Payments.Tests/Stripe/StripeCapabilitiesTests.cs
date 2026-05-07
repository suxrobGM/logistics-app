using Logistics.Infrastructure.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Tests.Stripe;

public class StripeCapabilitiesTests
{
    [Theory]
    [InlineData("US")]
    [InlineData("CA")]
    [InlineData("us")]
    public void ForCountry_NorthAmerica_RequestsAchAndCard(string country)
    {
        var caps = StripeCapabilities.ForCountry(country);

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.Transfers?.Requested);
        Assert.True(caps.UsBankAccountAchPayments?.Requested);
        Assert.Null(caps.SepaDebitPayments);
        Assert.Null(caps.BacsDebitPayments);
    }

    [Fact]
    public void ForCountry_GB_RequestsBacsAndCard_NotSepa()
    {
        var caps = StripeCapabilities.ForCountry("GB");

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.Transfers?.Requested);
        Assert.True(caps.BacsDebitPayments?.Requested);
        Assert.Null(caps.SepaDebitPayments);
        Assert.Null(caps.UsBankAccountAchPayments);
    }

    [Theory]
    [InlineData("DE")]
    [InlineData("AT")]
    public void ForCountry_DeAt_RequestsSepaGiropaySofort(string country)
    {
        var caps = StripeCapabilities.ForCountry(country);

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.SepaDebitPayments?.Requested);
        Assert.True(caps.GiropayPayments?.Requested);
        Assert.True(caps.SofortPayments?.Requested);
        Assert.Null(caps.IdealPayments);
        Assert.Null(caps.BancontactPayments);
    }

    [Fact]
    public void ForCountry_NL_RequestsSepaAndIdeal()
    {
        var caps = StripeCapabilities.ForCountry("NL");

        Assert.True(caps.SepaDebitPayments?.Requested);
        Assert.True(caps.IdealPayments?.Requested);
        Assert.Null(caps.GiropayPayments);
        Assert.Null(caps.BancontactPayments);
    }

    [Fact]
    public void ForCountry_BE_RequestsSepaAndBancontact()
    {
        var caps = StripeCapabilities.ForCountry("BE");

        Assert.True(caps.SepaDebitPayments?.Requested);
        Assert.True(caps.BancontactPayments?.Requested);
        Assert.Null(caps.IdealPayments);
        Assert.Null(caps.GiropayPayments);
    }

    [Theory]
    [InlineData("FR")]
    [InlineData("IT")]
    [InlineData("ES")]
    [InlineData("IE")]
    public void ForCountry_OtherEu_RequestsSepa_NoLocalMethods(string country)
    {
        var caps = StripeCapabilities.ForCountry(country);

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.SepaDebitPayments?.Requested);
        Assert.Null(caps.IdealPayments);
        Assert.Null(caps.BancontactPayments);
        Assert.Null(caps.GiropayPayments);
        Assert.Null(caps.SofortPayments);
        Assert.Null(caps.UsBankAccountAchPayments);
    }

    [Theory]
    [InlineData("CH")]
    [InlineData("NO")]
    [InlineData("IS")]
    [InlineData("LI")]
    public void ForCountry_NonSepaEurope_CardOnly(string country)
    {
        var caps = StripeCapabilities.ForCountry(country);

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.Transfers?.Requested);
        Assert.Null(caps.SepaDebitPayments);
        Assert.Null(caps.UsBankAccountAchPayments);
        Assert.Null(caps.BacsDebitPayments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ZZ")]
    public void ForCountry_UnknownOrEmpty_CardAndTransfersOnly(string? country)
    {
        var caps = StripeCapabilities.ForCountry(country);

        Assert.True(caps.CardPayments?.Requested);
        Assert.True(caps.Transfers?.Requested);
        Assert.Null(caps.SepaDebitPayments);
        Assert.Null(caps.UsBankAccountAchPayments);
        Assert.Null(caps.BacsDebitPayments);
    }
}
