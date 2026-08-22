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
    /// The counter allocates on its own: the Messages navigation is never written, because reading
    /// it on a tracked thread lazy-loads every stored RawBody.
    /// </summary>
    [Fact]
    public void AddMessage_LeavesTheMessagesNavigationUntouchedAndKeepsCounting()
    {
        var negotiation = NewThread();
        negotiation.AddOutboundMessage("offer");
        negotiation.AddInboundMessage("reply");

        Assert.Empty(negotiation.Messages);
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
