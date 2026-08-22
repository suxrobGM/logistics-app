using Logistics.Domain.Entities;
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
}
