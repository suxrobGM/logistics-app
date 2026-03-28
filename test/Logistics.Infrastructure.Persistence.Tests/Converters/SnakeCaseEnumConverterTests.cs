using Logistics.Infrastructure.Persistence.Converters;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Converters;

public class SnakeCaseEnumConverterTests
{
    #region Test enums

    public enum SimpleEnum { Draft, Active, Completed }
    public enum TwoWordEnum { InTransit, PickedUp, OnHold }
    public enum AcronymEnum { USD, US, API }
    public enum MixedAcronymEnum { ApiAccess, USDate, ELDProvider, HoSLog }
    public enum SingleValueEnum { Default }
    public enum NumberSuffixEnum { Gpt54, Version2 }

    #endregion

    #region Enum to snake_case (serialization)

    [Theory]
    [InlineData(SimpleEnum.Draft, "draft")]
    [InlineData(SimpleEnum.Active, "active")]
    [InlineData(SimpleEnum.Completed, "completed")]
    public void ToSnakeCase_SimpleWords(SimpleEnum value, string expected)
    {
        var converter = new SnakeCaseEnumConverter<SimpleEnum>();
        var result = ToProvider(converter, value);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TwoWordEnum.InTransit, "in_transit")]
    [InlineData(TwoWordEnum.PickedUp, "picked_up")]
    [InlineData(TwoWordEnum.OnHold, "on_hold")]
    public void ToSnakeCase_MultiWordValues(TwoWordEnum value, string expected)
    {
        var converter = new SnakeCaseEnumConverter<TwoWordEnum>();
        var result = ToProvider(converter, value);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AcronymEnum.USD, "usd")]
    [InlineData(AcronymEnum.US, "us")]
    [InlineData(AcronymEnum.API, "api")]
    public void ToSnakeCase_Acronyms_NotSplit(AcronymEnum value, string expected)
    {
        var converter = new SnakeCaseEnumConverter<AcronymEnum>();
        var result = ToProvider(converter, value);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(MixedAcronymEnum.ApiAccess, "api_access")]
    [InlineData(MixedAcronymEnum.USDate, "us_date")]
    [InlineData(MixedAcronymEnum.ELDProvider, "eld_provider")]
    [InlineData(MixedAcronymEnum.HoSLog, "ho_s_log")]
    public void ToSnakeCase_MixedAcronyms(MixedAcronymEnum value, string expected)
    {
        var converter = new SnakeCaseEnumConverter<MixedAcronymEnum>();
        var result = ToProvider(converter, value);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToSnakeCase_SingleValue()
    {
        var converter = new SnakeCaseEnumConverter<SingleValueEnum>();
        var result = ToProvider(converter, SingleValueEnum.Default);
        Assert.Equal("default", result);
    }

    [Theory]
    [InlineData(NumberSuffixEnum.Gpt54, "gpt54")]
    [InlineData(NumberSuffixEnum.Version2, "version2")]
    public void ToSnakeCase_NumberSuffix(NumberSuffixEnum value, string expected)
    {
        var converter = new SnakeCaseEnumConverter<NumberSuffixEnum>();
        var result = ToProvider(converter, value);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Snake_case to enum (deserialization)

    [Theory]
    [InlineData("draft", SimpleEnum.Draft)]
    [InlineData("active", SimpleEnum.Active)]
    [InlineData("completed", SimpleEnum.Completed)]
    public void FromSnakeCase_SimpleWords(string input, SimpleEnum expected)
    {
        var converter = new SnakeCaseEnumConverter<SimpleEnum>();
        var result = FromProvider(converter, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("in_transit", TwoWordEnum.InTransit)]
    [InlineData("picked_up", TwoWordEnum.PickedUp)]
    [InlineData("on_hold", TwoWordEnum.OnHold)]
    public void FromSnakeCase_MultiWordValues(string input, TwoWordEnum expected)
    {
        var converter = new SnakeCaseEnumConverter<TwoWordEnum>();
        var result = FromProvider(converter, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("usd", AcronymEnum.USD)]
    [InlineData("us", AcronymEnum.US)]
    [InlineData("api", AcronymEnum.API)]
    public void FromSnakeCase_Acronyms(string input, AcronymEnum expected)
    {
        var converter = new SnakeCaseEnumConverter<AcronymEnum>();
        var result = FromProvider(converter, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("api_access", MixedAcronymEnum.ApiAccess)]
    [InlineData("eld_provider", MixedAcronymEnum.ELDProvider)]
    public void FromSnakeCase_MixedAcronyms(string input, MixedAcronymEnum expected)
    {
        var converter = new SnakeCaseEnumConverter<MixedAcronymEnum>();
        var result = FromProvider(converter, input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Roundtrip

    [Theory]
    [InlineData(SimpleEnum.Draft)]
    [InlineData(SimpleEnum.Active)]
    [InlineData(SimpleEnum.Completed)]
    public void Roundtrip_SimpleEnum(SimpleEnum value)
    {
        var converter = new SnakeCaseEnumConverter<SimpleEnum>();
        var serialized = ToProvider(converter, value);
        var deserialized = FromProvider(converter, serialized);
        Assert.Equal(value, deserialized);
    }

    [Theory]
    [InlineData(TwoWordEnum.InTransit)]
    [InlineData(TwoWordEnum.PickedUp)]
    [InlineData(TwoWordEnum.OnHold)]
    public void Roundtrip_MultiWordEnum(TwoWordEnum value)
    {
        var converter = new SnakeCaseEnumConverter<TwoWordEnum>();
        var serialized = ToProvider(converter, value);
        var deserialized = FromProvider(converter, serialized);
        Assert.Equal(value, deserialized);
    }

    [Theory]
    [InlineData(AcronymEnum.USD)]
    [InlineData(AcronymEnum.US)]
    [InlineData(AcronymEnum.API)]
    public void Roundtrip_AcronymEnum(AcronymEnum value)
    {
        var converter = new SnakeCaseEnumConverter<AcronymEnum>();
        var serialized = ToProvider(converter, value);
        var deserialized = FromProvider(converter, serialized);
        Assert.Equal(value, deserialized);
    }

    #endregion

    #region Helpers

    private static string ToProvider<T>(SnakeCaseEnumConverter<T> converter, T value)
        where T : struct, Enum
    {
        // Access the converter's ConvertToProviderExpression and compile it
        var func = converter.ConvertToProviderExpression.Compile();
        return func(value);
    }

    private static T FromProvider<T>(SnakeCaseEnumConverter<T> converter, string value)
        where T : struct, Enum
    {
        var func = converter.ConvertFromProviderExpression.Compile();
        return func(value);
    }

    #endregion
}
