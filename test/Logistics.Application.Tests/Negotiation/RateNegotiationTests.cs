using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class RateNegotiationTests
{
    private static RateNegotiation NewThread() =>
        RateNegotiation.Create(Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None);

    /// <summary>
    /// The Messages navigation is never written, because reading it on a tracked thread lazy-loads
    /// every stored RawBody.
    /// </summary>
    [Fact]
    public void AddMessage_LeavesTheMessagesNavigationUntouchedAndKeepsTheReservedSequence()
    {
        var negotiation = NewThread();
        negotiation.AddOutboundMessage(1, "offer");

        Assert.Equal(2, negotiation.AddInboundMessage(2, "reply").Sequence);
        Assert.Empty(negotiation.Messages);
    }

    /// <summary>
    /// A counter-offer and an inbound reply on the same thread used to read the same in-memory
    /// counter, and the second save hit the unique (negotiation, sequence) index.
    /// </summary>
    [Fact]
    public async Task ReserveSequenceAsync_AdvancesTheCounterInTheDatabase()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var negotiation = NewThread();
        FormattableString? sql = null;
        tenantUow.SqlQueryAsync<int>(Arg.Do<FormattableString>(s => sql = s), Arg.Any<CancellationToken>())
            .Returns([4]);

        Assert.Equal(4, await tenantUow.ReserveSequenceAsync(negotiation, CancellationToken.None));
        Assert.NotNull(sql);
        Assert.Contains("UPDATE rate_negotiations SET last_sequence = last_sequence + 1", sql.Format);
        Assert.Equal([negotiation.Id], sql.GetArguments());
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
