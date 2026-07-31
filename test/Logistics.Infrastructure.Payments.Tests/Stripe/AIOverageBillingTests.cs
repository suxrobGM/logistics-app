using Logistics.Application.Abstractions.Payments.Stripe;
using Xunit;

namespace Logistics.Infrastructure.Payments.Tests.Stripe;

public class AIOverageBillingTests
{
    [Fact]
    public void UnitsFor_ZeroCost_BillsTheMinimumUnit()
    {
        Assert.Equal(1, AIOverageBilling.UnitsFor(0m));
    }

    [Theory]
    [InlineData(0.01, 1)]  // 0.03 marked-up -> ceil to 1 unit
    [InlineData(0.03, 1)]  // 0.09 -> 1 unit
    [InlineData(0.04, 2)]  // 0.12 -> 2 units
    [InlineData(0.10, 3)]  // 0.30 -> exactly 3 units, no overshoot
    [InlineData(0.11, 4)]  // 0.33 -> 4 units
    [InlineData(1.00, 30)] // 3.00 -> 30 units
    public void UnitsFor_MarksUpCostAndRoundsUpToWholeUnits(decimal costUsd, int expectedUnits)
    {
        Assert.Equal(expectedUnits, AIOverageBilling.UnitsFor(costUsd));
    }

    [Fact]
    public void UnitsFor_LargeCost_ScalesLinearly()
    {
        // $50 of model cost at 3x markup over $0.10 units.
        Assert.Equal(1500, AIOverageBilling.UnitsFor(50m));
    }
}
