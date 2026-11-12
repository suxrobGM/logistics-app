using Logistics.Infrastructure.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Tests.Stripe;

public class StripeMoneyConversionTests
{
    public static TheoryData<decimal, string, long> ToMinorUnits_TwoDecimalCurrencies_ScalesByHundredCases => new()
    {
        { 10.00m, "USD", 1000 },
        { 0.01m, "USD", 1 },
        { 123.45m, "EUR", 12345 },
        { 1.005m, "USD", 101 }, // banker's-style? AwayFromZero rounds half up
        { 0.00m, "USD", 0 },
        { -5.00m, "USD", -500 },
    };

    [Theory]
    [MemberData(nameof(ToMinorUnits_TwoDecimalCurrencies_ScalesByHundredCases))]
    public void ToMinorUnits_TwoDecimalCurrencies_ScalesByHundred(decimal amount, string currency, long expected)
    {
        Assert.Equal(expected, StripeMoneyConversion.ToMinorUnits(amount, currency));
    }

    public static TheoryData<decimal, string, long> ToMinorUnits_ZeroDecimalCurrencies_NoScalingCases => new()
    {
        { 1000m, "JPY", 1000 },
        { 150m, "KRW", 150 },
        { 500m, "VND", 500 },
        { 1.6m, "JPY", 2 }, // rounds to nearest whole unit
        { 1.4m, "JPY", 1 },
    };

    [Theory]
    [MemberData(nameof(ToMinorUnits_ZeroDecimalCurrencies_NoScalingCases))]
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

    public static TheoryData<long, string, decimal> FromMinorUnits_TwoDecimalCurrencies_DividesByHundredCases => new()
    {
        { 1234, "USD", 12.34m },
        { 0, "USD", 0m },
        { 1, "USD", 0.01m },
    };

    [Theory]
    [MemberData(nameof(FromMinorUnits_TwoDecimalCurrencies_DividesByHundredCases))]
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
