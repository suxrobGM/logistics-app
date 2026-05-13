using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class GeotabMapperTests
{
    [Theory]
    [InlineData("driving", HosViolationType.Driving11Hour)]
    [InlineData("11_hour", HosViolationType.Driving11Hour)]
    [InlineData("shift", HosViolationType.OnDuty14Hour)]
    [InlineData("break", HosViolationType.Break30Minute)]
    [InlineData("cycle", HosViolationType.Cycle70Hour)]
    [InlineData("restart", HosViolationType.RestartRequired)]
    [InlineData("unknown", HosViolationType.FormAndMannerViolation)]
    public void MapViolationType_UsRegion_ReturnsFmcsaValue(string input, HosViolationType expected)
    {
        Assert.Equal(expected, GeotabMapper.MapViolationType(input, Region.US));
    }

    [Theory]
    [InlineData("continuousdriving", HosViolationType.EuContinuousDriving4_5h)]
    [InlineData("4_5_hour", HosViolationType.EuContinuousDriving4_5h)]
    [InlineData("dailydriving", HosViolationType.EuDailyDriving9h)]
    [InlineData("weeklydriving", HosViolationType.EuWeeklyDriving56h)]
    [InlineData("biweeklydriving", HosViolationType.EuBiweeklyDriving90h)]
    [InlineData("dailyrest", HosViolationType.EuDailyRest11h)]
    [InlineData("weeklyrest", HosViolationType.EuWeeklyRest45h)]
    [InlineData("unknown", HosViolationType.EuFormAndManner)]
    public void MapViolationType_EuRegion_ReturnsEuValue(string input, HosViolationType expected)
    {
        Assert.Equal(expected, GeotabMapper.MapViolationType(input, Region.EU));
    }

    [Fact]
    public void MapViolationType_SameInputDifferentRegions_ReturnsDifferentEnumFamilies()
    {
        var us = GeotabMapper.MapViolationType("unknown", Region.US);
        var eu = GeotabMapper.MapViolationType("unknown", Region.EU);

        Assert.True((int)us < 100, "FMCSA values should be in 1–99 range");
        Assert.True((int)eu >= 100, "EU values should be in 100–199 range");
    }

    [Theory]
    [InlineData("driving", DutyStatus.Driving)]
    [InlineData("D", DutyStatus.Driving)]
    [InlineData("off", DutyStatus.OffDuty)]
    [InlineData("sleeperberth", DutyStatus.SleeperBerth)]
    [InlineData("on_duty", DutyStatus.OnDutyNotDriving)]
    [InlineData("personalconveyance", DutyStatus.PersonalConveyance)]
    [InlineData("yardmove", DutyStatus.YardMove)]
    [InlineData(null, DutyStatus.OffDuty)]
    public void MapDutyStatus_HandlesGeotabVariants(string? input, DutyStatus expected)
    {
        Assert.Equal(expected, GeotabMapper.MapDutyStatus(input));
    }
}
