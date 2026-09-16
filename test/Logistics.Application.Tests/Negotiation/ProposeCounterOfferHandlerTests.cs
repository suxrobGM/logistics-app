using System.Linq.Expressions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Application.Tests.TestKit;
using Logistics.Shared.Models;
using MockQueryable;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class ProposeCounterOfferHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IInboundEmailRouteRegistry _routeRegistry = Substitute.For<IInboundEmailRouteRegistry>();
    private readonly IBrokerCreditService _brokerCreditService = Substitute.For<IBrokerCreditService>();
    private readonly ILaneRateFloorResolver _floorResolver = Substitute.For<ILaneRateFloorResolver>();
    private readonly INegotiationEmailComposer _composer = Substitute.For<INegotiationEmailComposer>();
    private readonly IThreadedEmailSender _emailSender = Substitute.For<IThreadedEmailSender>();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<LoadBoardListing, Guid> _listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly ITenantRepository<RateNegotiation, Guid> _negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<NegotiationMessage, Guid> _messageRepo =
        Substitute.For<ITenantRepository<NegotiationMessage, Guid>>();
    private readonly ITenantRepository<AgentDecision, Guid> _decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly Tenant _tenant;
    private readonly LoadBoardListing _listing;
    private readonly ProposeCounterOfferCommand _command;
    private readonly ProposeCounterOfferHandler _sut;

    public ProposeCounterOfferHandlerTests()
    {
        _tenant = TestTenant.Create(companyName: "Test Carrier", mcNumber: "MC999");

        _listing = CreateListing();
        _command = new ProposeCounterOfferCommand
        {
            ListingId = _listing.Id,
            ProposedTotalRate = 2200m,
            ProposedRatePerMile = 2.20m,
            Message = "We can cover this at $2,200."
        };

        _tenantUow.Repository<LoadBoardListing>().Returns(_listingRepo);
        _tenantUow.Repository<RateNegotiation>().Returns(_negotiationRepo);
        _tenantUow.Repository<NegotiationMessage>().Returns(_messageRepo);
        _messageRepo.Query().Returns(new List<NegotiationMessage>().BuildMock());
        _tenantUow.Repository<AgentDecision>().Returns(_decisionRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);

        _listingRepo.GetByIdAsync(_listing.Id, Arg.Any<CancellationToken>()).Returns(_listing);
        SetupActiveNegotiation(null);

        // No credit record: the gate passes and stamps nothing, so "persisted nothing" stays readable.
        _brokerCreditService.GetBrokerCreditAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((BrokerCreditDto?)null);

        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 2.00m,
            Source = RateFloorSource.LaneExact,
            EffectiveFloorTotal = 2000m,
            ListingBelowFloor = true
        });

        _emailSender.ReplyDomain.Returns("mail.test.com");
        _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(true, "resend-1"));

        _composer.ComposeAsync(Arg.Any<ComposeNegotiationEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => new ComposedNegotiationEmail(
                "Rate offer: Dallas, TX -> Chicago, IL - NEG-1",
                "<p>offer</p>",
                ci.Arg<ComposeNegotiationEmailRequest>().AgentMessage));

        _sut = new ProposeCounterOfferHandler(
            _tenantUow, _routeRegistry, _brokerCreditService, _floorResolver, _composer, _emailSender,
            _broadcastService, NullLogger<ProposeCounterOfferHandler>.Instance);
    }

    private static LoadBoardListing CreateListing() => new()
    {
        ExternalListingId = "EXT-1",
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = new Address
        {
            Line1 = "1 St",
            City = "Dallas",
            State = "TX",
            ZipCode = "75001",
            Country = "US"
        },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address
        {
            Line1 = "2 St",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            Country = "US"
        },
        DestinationLocation = new GeoPoint(-87.6, 41.9),
        Distance = 1000,
        TotalRate = new Money { Amount = 1800m, Currency = "USD" },
        RatePerMile = 1.80m,
        BrokerName = "Test Broker",
        BrokerEmail = "broker@example.com",
        BrokerMcNumber = "MC123456",
        ExpiresAt = DateTime.UtcNow.AddDays(1)
    };

    private void SetupFloor(EffectiveRateFloorDto floor) =>
        _floorResolver.ResolveAsync(Arg.Any<LoadBoardListing>(), Arg.Any<CancellationToken>()).Returns(floor);

    private void SetupActiveNegotiation(RateNegotiation? negotiation) =>
        _negotiationRepo.GetAsync(Arg.Any<Expression<Func<RateNegotiation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(negotiation);

    /// <summary>A thread already in flight, carrying the floor snapshot it opened against.</summary>
    private RateNegotiation ExistingThread(decimal floorTotal)
    {
        var existing = RateNegotiation.Create(
            _listing.Id,
            "broker@example.com",
            new RateFloorSnapshot(
                2.00m, new Money { Amount = floorTotal, Currency = "USD" }, RateFloorSource.LaneExact));

        SetupActiveNegotiation(existing);
        return existing;
    }

    private void SetupCredit(int? score, bool? authorityActive = true) =>
        _brokerCreditService.GetBrokerCreditAsync(_listing.BrokerMcNumber, Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditDto
            {
                McNumber = "123456",
                CreditScore = score,
                DaysToPay = 30,
                AuthorityActive = authorityActive,
                Source = BrokerCreditSource.Demo,
                CheckedAt = DateTime.UtcNow
            });

    private Task AssertNothingSent() =>
        _emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

    private ThreadedEmail SentEmail() =>
        (ThreadedEmail)_emailSender.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(IThreadedEmailSender.SendAsync))
            .GetArguments()[0]!;

    private NegotiationMessage StoredMessage() =>
        (NegotiationMessage)_messageRepo.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(ITenantRepository<NegotiationMessage, Guid>.AddAsync))
            .GetArguments()[0]!;

    /// <summary>RFC 5322 msg-id: angle-bracketed, exactly one @, no whitespace inside.</summary>
    private static void AssertMsgId(string? value) =>
        Assert.Matches(@"^<[^<>@\s]+@[^<>@\s]+>$", value);

    #region Listing guards

    [Fact]
    public async Task Handle_ListingNotAvailable_Fails()
    {
        _listing.Status = LoadBoardListingStatus.Booked;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_NoBrokerEmail_Fails()
    {
        _listing.BrokerEmail = null;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("broker email", result.Error, StringComparison.OrdinalIgnoreCase);
        await AssertNothingSent();
    }

    #endregion

    #region Credit gate

    [Fact]
    public async Task Handle_CreditBelowThreshold_BlocksWithErrorCode()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_InactiveAuthority_Blocks()
    {
        SetupCredit(score: 90, authorityActive: false);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
    }

    #endregion

    #region Floor gate

    [Fact]
    public async Task Handle_NoFloorConfigured_FailsWithFloorMissing()
    {
        SetupFloor(new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None });

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationFloorMissing, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_OfferBelowFloor_FailsWithBelowFloor()
    {
        _command.ProposedTotalRate = 1900m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_OfferAtFloor_Sends()
    {
        _command.ProposedTotalRate = 2000m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_PerMileOnlyFloorAndNoDistance_FailsWithFloorMissing()
    {
        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 2.00m,
            Source = RateFloorSource.TenantDefault
        });
        _command.ProposedRatePerMile = null;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationFloorMissing, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_PerMileOnlyFloorBelowRate_FailsWithBelowFloor()
    {
        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 2.50m,
            Source = RateFloorSource.TenantDefault
        });

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_LaterRound_ChecksTheThreadSnapshotNotAFreshFloor()
    {
        ExistingThread(floorTotal: 2000m);
        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 3.00m,
            Source = RateFloorSource.LaneExact,
            EffectiveFloorTotal = 3000m
        });

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _floorResolver.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default);
    }

    [Fact]
    public async Task Handle_LaterRoundBelowThreadSnapshot_FailsWithBelowFloor()
    {
        ExistingThread(floorTotal: 2500m);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_FirstOffer_SnapshotsTheFloorItWasCheckedAgainst()
    {
        // Per-mile x distance (2000) beats the flat total (1000), so 2000 is what was enforced.
        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 2.00m,
            MinTotalRate = new Money { Amount = 1000m, Currency = "USD" },
            Source = RateFloorSource.LaneExact,
            EffectiveFloorTotal = 2000m
        });

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _negotiationRepo.Received(1).AddAsync(
            Arg.Is<RateNegotiation>(n => n.FloorTotalRate!.Amount == 2000m && n.FloorRatePerMile == 2.00m),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Thread lifecycle

    [Fact]
    public async Task Handle_RoundCapReached_FailsWithoutSending()
    {
        var existing = ExistingThread(floorTotal: 2000m);
        existing.RoundCount = RateNegotiation.MaxRounds;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("rounds", result.Error, StringComparison.OrdinalIgnoreCase);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_EmailSendFails_PersistsNothing()
    {
        _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _negotiationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailSendFails_RevokesTheRouteItOpened()
    {
        _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _routeRegistry.Received(1).RevokeAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FirstOffer_OpensTheReplyRouteBeforeSending()
    {
        await _sut.Handle(_command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _routeRegistry.OpenAsync(
                Arg.Any<string>(), _tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
            _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_FirstOffer_PersistsThreadMessageAndRoute()
    {
        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.RoundCount);
        Assert.Equal(RateNegotiationStatus.AwaitingBroker, result.Value.Status);

        await _negotiationRepo.Received(1).AddAsync(Arg.Any<RateNegotiation>(), Arg.Any<CancellationToken>());
        await _messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m =>
                m.Direction == NegotiationMessageDirection.Outbound &&
                m.ProposedTotalRate!.Amount == 2200m),
            Arg.Any<CancellationToken>());
        await _routeRegistry.Received(1).OpenAsync(
            Arg.Any<string>(), _tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _broadcastService.Received(1).BroadcastNegotiationAsync(_tenant.Id, Arg.Any<RateNegotiationDto>());
    }

    [Fact]
    public async Task Handle_FirstOffer_SendsToListingBrokerWithThreadReplyAddress()
    {
        await _sut.Handle(_command, CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e =>
                e.To == "broker@example.com" &&
                e.ReplyTo.StartsWith("offer-") &&
                e.ReplyTo.EndsWith("@mail.test.com") &&
                e.InReplyToMessageId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FirstOffer_SendsUnderAWellFormedMessageId()
    {
        await _sut.Handle(_command, CancellationToken.None);

        var sent = SentEmail();
        AssertMsgId(sent.MessageId);
        Assert.EndsWith("@mail.test.com>", sent.MessageId);
    }

    /// <summary>
    /// The provider's id is a bare handle, not a msg-id; storing it unthreads every later round.
    /// </summary>
    [Fact]
    public async Task Handle_FirstOffer_StoresTheSentMessageIdNotTheProviderId()
    {
        await _sut.Handle(_command, CancellationToken.None);

        var stored = StoredMessage();
        Assert.Equal(SentEmail().MessageId, stored.RfcMessageId);
        Assert.NotEqual("resend-1", stored.RfcMessageId);
    }

    [Fact]
    public async Task Handle_SecondOffer_ChainsThreadHeadersAndReusesRoute()
    {
        var existing = ExistingThread(floorTotal: 2000m);
        var first = existing.AddOutboundMessage("first offer");
        first.RfcMessageId = "<neg-0@mail.test.com>";
        var reply = existing.AddInboundMessage("we want more", rfcMessageId: "<CAF-1@broker.example.com>");
        _messageRepo.Query().Returns(new List<NegotiationMessage> { first, reply }.BuildMock());

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sent = SentEmail();
        Assert.Equal("<CAF-1@broker.example.com>", sent.InReplyToMessageId);
        Assert.Equal("<neg-0@mail.test.com> <CAF-1@broker.example.com>", sent.References);
        AssertMsgId(sent.InReplyToMessageId);
        Assert.All(sent.References!.Split(' '), AssertMsgId);
        await _negotiationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _routeRegistry.DidNotReceiveWithAnyArgs().OpenAsync(default!, default, default);
        await _routeRegistry.Received(1).RefreshAsync(
            existing.ReplyToken, _tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDecisionId_BackfillsNegotiationOnDecision()
    {
        var decision = new AgentDecision { Type = AgentDecisionType.Query };
        _decisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        _command.DecisionId = decision.Id;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value!.Id, decision.NegotiationId);
    }

    #endregion
}
