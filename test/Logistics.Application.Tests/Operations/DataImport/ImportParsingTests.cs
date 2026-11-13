using Logistics.Application.Modules.Operations.DataImport.Importers;
using Logistics.Application.Modules.Operations.DataImport.Services;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.Operations.DataImport;

public class ImportParsingTests
{
    #region Numbers and dates

    public static TheoryData<string, decimal> FormattedAmounts => new()
    {
        { "$1,250.00", 1250.00m },
        { "1 250,50 €", 1250.50m },
        { "1.234", 1234m },
        { "12,5", 12.5m },
        { "900", 900m },
    };

    [Theory]
    [MemberData(nameof(FormattedAmounts))]
    public void ParseDecimal_FormattedAmounts_ParsesValue(string value, decimal expected)
    {
        Assert.Equal(expected, ImportValueParser.ParseDecimal(value));
    }

    [Fact]
    public void ParseDecimal_Text_ReturnsNull()
    {
        Assert.Null(ImportValueParser.ParseDecimal("TBD"));
    }

    [Theory]
    [InlineData("03/04/2026", DateFormatType.US, 3, 4)]
    [InlineData("03/04/2026", DateFormatType.European, 4, 3)]
    [InlineData("2026-03-04", DateFormatType.European, 3, 4)]
    public void ParseDate_FollowsTenantOrder(string value, DateFormatType format, int month, int day)
    {
        var date = ImportValueParser.ParseDate(value, format);

        Assert.Equal(new DateTime(2026, month, day), date!.Value.Date);
        Assert.Equal(DateTimeKind.Utc, date.Value.Kind);
    }

    public static TheoryData<string, Region, decimal, decimal> ToMeters_HeaderUnitWinsOverRegionCases => new()
    {
        { "Length (ft)", Region.EU, 53m, 16.15m },
        { "Length (m)", Region.US, 13.6m, 13.6m },
        { "Length", Region.US, 53m, 16.15m },
        { "Length", Region.EU, 13.6m, 13.6m },
    };

    [Theory]
    [MemberData(nameof(ToMeters_HeaderUnitWinsOverRegionCases))]
    public void ToMeters_HeaderUnitWinsOverRegion(string header, Region region, decimal length, decimal expected)
    {
        Assert.Equal(expected, ImportValueParser.ToMeters(length, header, region));
    }

    [Theory]
    [InlineData("Dry Van", TrailerType.DryVan)]
    [InlineData("step_deck", TrailerType.StepDeck)]
    [InlineData("V", TrailerType.DryVan)]
    [InlineData("Container Chassis", TrailerType.Chassis)]
    public void ParseEnum_NamesDescriptionsAndAliases_Resolve(string value, TrailerType expected)
    {
        Assert.Equal(expected, ImportValueParser.ParseEnum(value, TrailerImporter.TypeAliases));
    }

    #endregion

    #region Addresses

    [Fact]
    public void Build_FullUsAddress_SplitsIntoParts()
    {
        var address = ImportAddressParser.Build(null, null, null, null, null, null,
            "123 Main St, Suite 4, Dallas, TX 75201, USA", "US");

        Assert.NotNull(address);
        Assert.Equal("123 Main St, Suite 4", address.Line1);
        Assert.Equal("Dallas", address.City);
        Assert.Equal("TX", address.State);
        Assert.Equal("75201", address.ZipCode);
        Assert.Equal("US", address.Country);
    }

    [Fact]
    public void Build_CaliforniaWithoutZip_IsNotReadAsCanada()
    {
        var address = ImportAddressParser.Build(null, null, null, null, null, null,
            "500 Harbor Blvd, Los Angeles, CA", "US");

        Assert.Equal("CA", address!.State);
        Assert.Equal("US", address.Country);
    }

    [Fact]
    public void Build_EuropeanPostcodeFirst_ParsesZipAndCity()
    {
        var address = ImportAddressParser.Build(null, null, null, "Berlin", null, "Germany",
            "Invalidenstr. 1, 10115 Berlin", "DE");

        Assert.Equal("Invalidenstr. 1", address!.Line1);
        Assert.Equal("10115", address.ZipCode);
        Assert.Equal("Berlin", address.City);
        Assert.Equal("DE", address.Country);
    }

    [Fact]
    public void Build_SeparateColumnsWin_FullAddressOnlyFillsGaps()
    {
        var address = ImportAddressParser.Build("9 Elm St", null, "Austin", null, null, null,
            "1 Other Rd, Houston, TX 77001", "US");

        Assert.Equal("9 Elm St", address!.Line1);
        Assert.Equal("Austin", address.City);
        Assert.Equal("TX", address.State);
        Assert.Equal("77001", address.ZipCode);
    }

    [Fact]
    public void Build_NoAddressCells_ReturnsNull()
    {
        Assert.Null(ImportAddressParser.Build(null, null, null, null, null, null, null, "US"));
    }

    #endregion
}
