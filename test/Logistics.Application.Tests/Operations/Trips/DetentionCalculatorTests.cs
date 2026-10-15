using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class DetentionCalculatorTests
{
    private static readonly DateTime Arrived = new(2026, 10, 12, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DetentionTerms TwoHoursAt50 = new(120, 50m);

    [Theory]
    [InlineData(120, 0, 0)]       // exactly the free time
    [InlineData(134, 0, 0)]       // 14 minutes over: under one billing increment
    [InlineData(135, 0.25, 12.5)] // 15 minutes over
    [InlineData(200, 1.25, 62.5)] // 80 minutes over: rounds down to 75
    [InlineData(60, 0, 0)]        // inside free time
    public void Calculate_Departed_BillsWholeQuarterHoursPastFreeTime(int dwellMinutes, decimal hours, decimal charge)
    {
        var detention = DetentionCalculator.Calculate(Arrived, Arrived.AddMinutes(dwellMinutes), Arrived, TwoHoursAt50);

        Assert.Equal(dwellMinutes, detention.DwellMinutes);
        Assert.Equal(hours, detention.BillableHours);
        Assert.Equal(charge, detention.Charge);
        Assert.False(detention.IsOngoing);
    }

    [Fact]
    public void Calculate_FreeTimeBoundary_IsNotOverFreeTimeUntilAMinutePast()
    {
        Assert.False(DetentionCalculator.Calculate(Arrived, Arrived.AddMinutes(120), Arrived, TwoHoursAt50).IsOverFreeTime);
        Assert.True(DetentionCalculator.Calculate(Arrived, Arrived.AddMinutes(121), Arrived, TwoHoursAt50).IsOverFreeTime);
    }

    [Fact]
    public void Calculate_MissingDeparture_MeasuresToNowAndIsOngoing()
    {
        var now = Arrived.AddHours(3);

        var detention = DetentionCalculator.Calculate(Arrived, null, now, TwoHoursAt50);

        Assert.True(detention.IsOngoing);
        Assert.Equal(180, detention.DwellMinutes);
        Assert.Equal(1m, detention.BillableHours);
    }

    [Fact]
    public void Calculate_NoRate_TracksButChargesNothing()
    {
        var detention = DetentionCalculator.Calculate(Arrived, Arrived.AddHours(4), Arrived, new DetentionTerms(120, null));

        Assert.Equal(2m, detention.BillableHours);
        Assert.Equal(0m, detention.Charge);
    }

    [Fact]
    public void For_CustomerTermsOverrideTenantFieldByField()
    {
        var tenant = new TenantSettings { DetentionFreeMinutes = 90, DetentionRatePerHour = 40m };
        var customer = new Customer { Name = "Acme", DetentionRatePerHour = 75m };

        var terms = DetentionTerms.For(tenant, customer);

        Assert.Equal(90, terms.FreeMinutes);
        Assert.Equal(75m, terms.RatePerHour);
    }

    [Fact]
    public void For_NothingConfigured_UsesTwoHoursAndNoRate()
    {
        var terms = DetentionTerms.For(new TenantSettings(), null);

        Assert.Equal(120, terms.FreeMinutes);
        Assert.Null(terms.RatePerHour);
    }
}
