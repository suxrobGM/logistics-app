using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;
using Logistics.Application.Modules.Compliance.Eld.Queries;

namespace Logistics.Application.Tests.Eld;

public class HosLimitsEndpointTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GetHosLimitsHandler sut;

    public HosLimitsEndpointTests()
    {
        sut = new GetHosLimitsHandler(tenantUow);
    }

    [Fact]
    public async Task Handle_UsTenant_ReturnsFmcsaLimits()
    {
        WireTenantRegion(Region.US);

        var result = await sut.Handle(new GetHosLimitsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(HosLimits.FmcsaCode, result.Value!.RuleSetCode);
        Assert.Equal(11 * 60, result.Value.MaxDailyDrivingMinutes);
        Assert.Equal(14 * 60, result.Value.MaxDailyOnDutyMinutes);
        Assert.Equal(70 * 60, result.Value.MaxBiweeklyDrivingMinutes);
        Assert.Null(result.Value.MaxContinuousDrivingMinutes);
        Assert.Equal(8, result.Value.CycleDays);
    }

    [Fact]
    public async Task Handle_EuTenant_ReturnsEu561Limits()
    {
        WireTenantRegion(Region.EU);

        var result = await sut.Handle(new GetHosLimitsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(HosLimits.Eu561Code, result.Value!.RuleSetCode);
        Assert.Equal(9 * 60, result.Value.MaxDailyDrivingMinutes);
        Assert.Equal(56 * 60, result.Value.MaxWeeklyDrivingMinutes);
        Assert.Equal(90 * 60, result.Value.MaxBiweeklyDrivingMinutes);
        Assert.Equal(270, result.Value.MaxContinuousDrivingMinutes);
        Assert.Equal(270, result.Value.RequiredBreakAfterMinutes);
        Assert.Equal(11 * 60, result.Value.MinDailyRestMinutes);
        Assert.Equal(45 * 60, result.Value.MinWeeklyRestMinutes);
        Assert.Equal(14, result.Value.CycleDays);
    }

    [Theory]
    [InlineData(Region.US, HosLimits.FmcsaCode)]
    [InlineData(Region.EU, HosLimits.Eu561Code)]
    public void RuleSetSelector_CodeFor_ReturnsExpectedCode(Region region, string expected)
    {
        Assert.Equal(expected, RuleSetSelector.CodeFor(region));
    }

    private void WireTenantRegion(Region region)
    {
        var tenant = new Tenant
        {
            Name = "test",
            CompanyAddress = new Address
            {
                Line1 = "1",
                City = "x",
                ZipCode = "0",
                State = "x",
                Country = region == Region.EU ? "DE" : "US"
            },
            ConnectionString = "x",
            BillingEmail = "x@y.z",
            Settings = new TenantSettings { Region = region }
        };
        tenantUow.GetCurrentTenant().Returns(tenant);
    }
}
