using Logistics.Infrastructure.Tax.Stripe;

namespace Logistics.Infrastructure.Tax.Tests.Stripe;

public class StripeMoneyConversionTests
{
    [Theory]
    [InlineData(10.00, "USD", 1000)]
    [InlineData(0.01, "USD", 1)]
    [InlineData(123.45, "EUR", 12345)]
    [InlineData(1.005, "USD", 101)]   // banker's-style? AwayFromZero rounds half up
    [InlineData(0.00, "USD", 0)]
    [InlineData(-5.00, "USD", -500)]
    public void ToMinorUnits_TwoDecimalCurrencies_ScalesByHundred(decimal amount, string currency, long expected)
    {
        Assert.Equal(expected, StripeMoneyConversion.ToMinorUnits(amount, currency));
    }

    [Theory]
    [InlineData(1000, "JPY", 1000)]
    [InlineData(150, "KRW", 150)]
    [InlineData(500, "VND", 500)]
    [InlineData(1.6, "JPY", 2)]   // rounds to nearest whole unit
    [InlineData(1.4, "JPY", 1)]
    public void ToMinorUnits_ZeroDecimalCurrencies_NoScaling(decimal amount, string currency, long expected)
    {
        Assert.Equal(expected, StripeMoneyConversion.ToMinorUnits(amount, currency));
    }

    [Theory]
    [InlineData("usd")]   // case-insensitive should treat usd same as USD
    [InlineData("USD")]
    public void ToMinorUnits_CurrencyCaseInsensitive(string currency)
    {
        Assert.Equal(100, StripeMoneyConversion.ToMinorUnits(1m, currency));
    }

    [Theory]
    [InlineData("jpy")]
    [InlineData("JPY")]
    public void ToMinorUnits_ZeroDecimalCurrencyCaseInsensitive(string currency)
    {
        Assert.Equal(100, StripeMoneyConversion.ToMinorUnits(100m, currency));
    }

    [Theory]
    [InlineData(1234, "USD", 12.34)]
    [InlineData(0, "USD", 0)]
    [InlineData(1, "USD", 0.01)]
    public void FromMinorUnits_TwoDecimalCurrencies_DividesByHundred(long minor, string currency, decimal expected)
    {
        Assert.Equal(expected, StripeMoneyConversion.FromMinorUnits(minor, currency));
    }

    [Theory]
    [InlineData(1234, "JPY", 1234)]
    [InlineData(150, "KRW", 150)]
    public void FromMinorUnits_ZeroDecimalCurrencies_NoScaling(long minor, string currency, decimal expected)
    {
        Assert.Equal(expected, StripeMoneyConversion.FromMinorUnits(minor, currency));
    }

    [Fact]
    public void RoundTrip_StaysAccurate()
    {
        var original = 99.99m;
        var minor = StripeMoneyConversion.ToMinorUnits(original, "USD");
        var back = StripeMoneyConversion.FromMinorUnits(minor, "USD");
        Assert.Equal(original, back);
    }
}
