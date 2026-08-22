using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class RateNegotiationTests
{
    private static RateNegotiation NewThread() =>
        RateNegotiation.Create(Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None);

    [Fact]
    public void AddMessage_NumbersEachOneOnce()
    {
        var negotiation = NewThread();

        var first = negotiation.AddOutboundMessage("offer");
        var second = negotiation.AddInboundMessage("reply");
        var third = negotiation.AddOutboundMessage("counter");

        Assert.Equal([1, 2, 3], new[] { first.Sequence, second.Sequence, third.Sequence });
    }

    /// <summary>
    /// The counter, not the Messages collection, is what allocates. A thread rehydrated without its
    /// messages must still hand out the next number rather than restart at 1.
    /// </summary>
    [Fact]
    public void AddMessage_ThreadLoadedWithoutMessages_ContinuesFromTheCounter()
    {
        var negotiation = NewThread();
        negotiation.AddOutboundMessage("offer");
        negotiation.AddInboundMessage("reply");
        negotiation.Messages.Clear();

        Assert.Equal(3, negotiation.AddOutboundMessage("counter").Sequence);
    }

    #region Expiry sweep predicate

    private static RateNegotiation LapsedThread(RateNegotiationStatus status)
    {
        var negotiation = NewThread();
        negotiation.Status = status;
        negotiation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        return negotiation;
    }

    /// <summary>An inbound reply flips the thread to BrokerReplied; it still has to expire.</summary>
    [Theory]
    [InlineData(RateNegotiationStatus.AwaitingBroker, true)]
    [InlineData(RateNegotiationStatus.BrokerReplied, true)]
    [InlineData(RateNegotiationStatus.Accepted, false)]
    [InlineData(RateNegotiationStatus.Expired, false)]
    public void LapsedAt_LapsedThread_MatchesOnlyOpenStatuses(RateNegotiationStatus status, bool expected)
    {
        var matches = RateNegotiation.LapsedAt(DateTime.UtcNow).Compile();

        Assert.Equal(expected, matches(LapsedThread(status)));
    }

    [Fact]
    public void LapsedAt_WindowStillOpen_DoesNotMatch()
    {
        var negotiation = NewThread();
        negotiation.ExpiresAt = DateTime.UtcNow.AddHours(1);

        Assert.False(RateNegotiation.LapsedAt(DateTime.UtcNow).Compile()(negotiation));
    }

    #endregion
}
