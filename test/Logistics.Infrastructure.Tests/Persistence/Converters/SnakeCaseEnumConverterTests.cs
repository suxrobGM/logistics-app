using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logistics.Infrastructure.Tests.Persistence.Converters;

public class SnakeCaseEnumConverterTests
{
    public enum NameShape { Draft, InTransit, USD, ApiAccess, USDate, ELDProvider, HoSLog, Gpt54 }

    public enum ShipmentState { None, InTransit, PickedUp }

    [Flags]
    public enum FlagsEnum { None = 0, A = 1, B = 2, C = 4 }

    [Theory]
    [InlineData(NameShape.Draft, "draft")]
    [InlineData(NameShape.InTransit, "in_transit")]
    [InlineData(NameShape.USD, "usd")]
    [InlineData(NameShape.ApiAccess, "api_access")]
    [InlineData(NameShape.USDate, "us_date")]
    [InlineData(NameShape.ELDProvider, "eld_provider")]
    [InlineData(NameShape.HoSLog, "ho_s_log")]
    [InlineData(NameShape.Gpt54, "gpt54")]
    public void ToProvider_WritesSnakeCase(NameShape value, string expected)
    {
        Assert.Equal(expected, ToProvider(new SnakeCaseEnumConverter<NameShape>(), value));
    }

    /// <summary>
    /// Empty and null read as the default: rows can carry a "" left over from an AddColumn
    /// migration (e.g. trucks.adr_equipment_allowed_classes).
    /// </summary>
    [Theory]
    [InlineData("in_transit", ShipmentState.InTransit)]
    [InlineData("IN_TRANSIT", ShipmentState.InTransit)]
    [InlineData("PickedUp", ShipmentState.PickedUp)]
    [InlineData("2", ShipmentState.PickedUp)]
    [InlineData("", ShipmentState.None)]
    [InlineData(null, ShipmentState.None)]
    public void FromProvider_ReadsAnyCaseNumbersAndBlanks(string? stored, ShipmentState expected)
    {
        Assert.Equal(expected, FromProvider(new SnakeCaseEnumConverter<ShipmentState>(), stored!));
    }

    public static TheoryData<Enum, string> StoredDomainNames => new()
    {
        { LicenseClass.EUC1E, "euc1e" },
        { HosViolationType.EUContinuousDriving4_5h, "eu_continuous_driving4_5h" },
    };

    /// <summary>
    /// Existing rows hold these strings. <c>euc1e</c> is a legacy spelling the naming policy would now
    /// write as <c>euc1_e</c>.
    /// </summary>
    [Theory]
    [MemberData(nameof(StoredDomainNames))]
    public void StoredDomainName_WritesAndReadsTheSameString(Enum value, string stored)
    {
        var converter = ConverterFor(value.GetType());

        Assert.Equal(stored, converter.ConvertToProvider(value));
        Assert.Equal(value, converter.ConvertFromProvider(stored));
    }

    /// <summary>Stored flag combinations separate the names with ", _".</summary>
    [Fact]
    public void FlagsCombination_WritesCommaUnderscoreSeparatorAndReadsBack()
    {
        var converter = new SnakeCaseEnumConverter<FlagsEnum>();

        Assert.Equal("a, _c", ToProvider(converter, FlagsEnum.A | FlagsEnum.C));
        Assert.Equal(FlagsEnum.A | FlagsEnum.C, FromProvider(converter, "a, _c"));
    }

    public static TheoryData<Type> DomainEnums()
    {
        var data = new TheoryData<Type>();
        foreach (var type in typeof(HosViolationType).Assembly.GetTypes().Where(t => t.IsEnum))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(DomainEnums))]
    public void Roundtrip_EveryDomainEnumMember(Type enumType)
    {
        var converter = ConverterFor(enumType);

        foreach (var value in Enum.GetValues(enumType))
        {
            var stored = converter.ConvertToProvider(value);
            Assert.Equal(value, converter.ConvertFromProvider(stored));
        }
    }

    private static ValueConverter ConverterFor(Type enumType) =>
        (ValueConverter)Activator.CreateInstance(typeof(SnakeCaseEnumConverter<>).MakeGenericType(enumType))!;

    private static string ToProvider<T>(SnakeCaseEnumConverter<T> converter, T value)
        where T : struct, Enum =>
        converter.ConvertToProviderExpression.Compile()(value);

    private static T FromProvider<T>(SnakeCaseEnumConverter<T> converter, string value)
        where T : struct, Enum =>
        converter.ConvertFromProviderExpression.Compile()(value);
}
