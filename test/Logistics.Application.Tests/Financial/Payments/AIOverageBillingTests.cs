using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Domain.Entities;
using Xunit;

namespace Logistics.Application.Tests.Financial.Payments;

public class AIOverageBillingTests
{
    [Fact]
    public void UnitsFor_ZeroCost_BillsTheMinimumUnit()
    {
        Assert.Equal(1, AIOverageBilling.UnitsFor(0m));
    }

    public static TheoryData<decimal, int> UnitsFor_MarksUpCostAndRoundsUpToWholeUnitsCases => new()
    {
        { 0.01m, 1 }, // 0.03 marked-up -> ceil to 1 unit
        { 0.03m, 1 }, // 0.09 -> 1 unit
        { 0.04m, 2 }, // 0.12 -> 2 units
        { 0.10m, 3 }, // 0.30 -> exactly 3 units, no overshoot
        { 0.11m, 4 }, // 0.33 -> 4 units
        { 1.00m, 30 }, // 3.00 -> 30 units
    };

    [Theory]
    [MemberData(nameof(UnitsFor_MarksUpCostAndRoundsUpToWholeUnitsCases))]
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

    [Fact]
    public void IsBillable_CompletedOverageSession_Bills()
    {
        var session = new AgentSession { IsOverage = true };
        session.Complete();

        Assert.True(AIOverageBilling.IsBillable(session));
    }

    [Fact]
    public void IsBillable_FailedOverageSession_DoesNotBill()
    {
        var session = new AgentSession { IsOverage = true };
        session.Fail("boom");

        Assert.False(AIOverageBilling.IsBillable(session));
    }

    [Fact]
    public void IsBillable_CompletedSessionUnderBudget_DoesNotBill()
    {
        var session = new AgentSession { IsOverage = false };
        session.Complete();

        Assert.False(AIOverageBilling.IsBillable(session));
    }
}
