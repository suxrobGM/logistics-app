using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Tax.Data;

namespace Logistics.Infrastructure.Tax.Tests.Data;

public class TaxJurisdictionsProviderTests
{
    private const string EUMembers =
        "AT BE BG HR CY CZ DK EE FI FR DE GR HU IE IT LV LT LU MT NL PL PT RO SK SI ES SE";

    private const string USStates =
        "AL AK AZ AR CA CO CT DE FL GA HI ID IL IN IA KS KY LA ME MD MA MI MN MS MO MT NE NV NH NJ " +
        "NM NY NC ND OH OK OR PA RI SC SD TN TX UT VT VA WA WV WI WY DC";

    private readonly TaxJurisdictionsProvider _sut = new();

    [Fact]
    public void GetSupportedJurisdictions_ListsEuropeThenUSStatesThenOtherCountries()
    {
        var codes = _sut.GetSupportedJurisdictions().Select(j => j.DisplayName);

        var expected = $"{EUMembers} NO IS CH LI GB "
                       + string.Join(' ', USStates.Split(' ').Select(s => $"US-{s}"))
                       + " AU NZ CA JP SG IN MX BR ZA";
        Assert.Equal(expected, string.Join(' ', codes));
    }

    [Fact]
    public void GetSupportedJurisdictions_EveryEuropeanAndUSRowHasARate()
    {
        var rows = _sut.GetSupportedJurisdictions().Where(j => j.Source != "country_default").ToList();

        Assert.Equal(83, rows.Count);
        Assert.All(rows, j => Assert.NotNull(j.DefaultRatePercent));
    }

    [Fact]
    public void GetAllowed_EU_IsTheMemberStatesPlusWiderEurope()
    {
        var allowed = RegionCountries.GetAllowed(Region.EU);

        Assert.Equal(44, allowed.Count);
        Assert.All(EUMembers.Split(' '), c => Assert.Contains(c, allowed));
        Assert.All(EUMembers.Split(' '), c => Assert.True(EUVatRules.IsEUMember(c)));
        Assert.False(EUVatRules.IsEUMember("GB"));
    }
}
