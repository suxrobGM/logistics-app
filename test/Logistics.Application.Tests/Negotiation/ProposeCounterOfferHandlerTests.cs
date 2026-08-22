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
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IInboundEmailRouteRegistry routeRegistry = Substitute.For<IInboundEmailRouteRegistry>();
    private readonly IBrokerCreditService brokerCreditService = Substitute.For<IBrokerCreditService>();
    private readonly ILaneRateFloorResolver floorResolver = Substitute.For<ILaneRateFloorResolver>();
    private readonly INegotiationEmailComposer composer = Substitute.For<INegotiationEmailComposer>();
    private readonly IThreadedEmailSender emailSender = Substitute.For<IThreadedEmailSender>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly ITenantRepository<RateNegotiation, Guid> negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<NegotiationMessage, Guid> messageRepo =
        Substitute.For<ITenantRepository<NegotiationMessage, Guid>>();
    private readonly ITenantRepository<AgentDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly Tenant tenant;
    private readonly LoadBoardListing listing;
    private readonly ProposeCounterOfferCommand command;
    private readonly ProposeCounterOfferHandler sut;

    public ProposeCounterOfferHandlerTests()
    {
        tenant = TestTenant.Create(companyName: "Test Carrier", mcNumber: "MC999");

        listing = CreateListing();
        command = new ProposeCounterOfferCommand
        {
            ListingId = listing.Id,
            ProposedTotalRate = 2200m,
            ProposedRatePerMile = 2.20m,
            Message = "We can cover this at $2,200."
        };

        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        tenantUow.Repository<RateNegotiation>().Returns(negotiationRepo);
        tenantUow.Repository<NegotiationMessage>().Returns(messageRepo);
        messageRepo.Query().Returns(new List<NegotiationMessage>().BuildMock());
        tenantUow.Repository<AgentDecision>().Returns(decisionRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);

        listingRepo.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        SetupActiveNegotiation(null);

        // No credit record: the gate passes and stamps nothing, so "persisted nothing" stays readable.
        brokerCreditService.GetBrokerCreditAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((BrokerCreditDto?)null);

        SetupFloor(new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = 2.00m,
            Source = RateFloorSource.LaneExact,
            EffectiveFloorTotal = 2000m,
            ListingBelowFloor = true
        });

        emailSender.ReplyDomain.Returns("mail.test.com");
        emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(true, "resend-1"));

        composer.ComposeAsync(Arg.Any<ComposeNegotiationEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => new ComposedNegotiationEmail(
                "Rate offer: Dallas, TX -> Chicago, IL - NEG-1",
                "<p>offer</p>",
                ci.Arg<ComposeNegotiationEmailRequest>().AgentMessage));

        sut = new ProposeCounterOfferHandler(
            tenantUow, routeRegistry, brokerCreditService, floorResolver, composer, emailSender,
            broadcastService, NullLogger<ProposeCounterOfferHandler>.Instance);
    }

    private static LoadBoardListing CreateListing() => new()
    {
        ExternalListingId = "EXT-1",
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = new Address
        {
            Line1 = "1 St", City = "Dallas", State = "TX", ZipCode = "75001", Country = "US"
        },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address
        {
            Line1 = "2 St", City = "Chicago", State = "IL", ZipCode = "60601", Country = "US"
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
        floorResolver.ResolveAsync(Arg.Any<LoadBoardListing>(), Arg.Any<CancellationToken>()).Returns(floor);

    private void SetupActiveNegotiation(RateNegotiation? negotiation) =>
        negotiationRepo.GetAsync(Arg.Any<Expression<Func<RateNegotiation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(negotiation);

    /// <summary>A thread already in flight, carrying the floor snapshot it opened against.</summary>
    private RateNegotiation ExistingThread(decimal floorTotal)
    {
        var existing = RateNegotiation.Create(
            listing.Id,
            "broker@example.com",
            new RateFloorSnapshot(
                2.00m, new Money { Amount = floorTotal, Currency = "USD" }, RateFloorSource.LaneExact));

        SetupActiveNegotiation(existing);
        return existing;
    }

    private void SetupCredit(int? score, bool? authorityActive = true) =>
        brokerCreditService.GetBrokerCreditAsync(listing.BrokerMcNumber, Arg.Any<CancellationToken>())
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
        emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

    #region Listing guards

    [Fact]
    public async Task Handle_ListingNotAvailable_Fails()
    {
        listing.Status = LoadBoardListingStatus.Booked;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_NoBrokerEmail_Fails()
    {
        listing.BrokerEmail = null;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("broker email", result.Error, StringComparison.OrdinalIgnoreCase);
        await AssertNothingSent();
    }

    #endregion

    #region Credit gate

    [Fact]
    public async Task Handle_CreditBelowThreshold_BlocksWithErrorCode()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_InactiveAuthority_Blocks()
    {
        SetupCredit(score: 90, authorityActive: false);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
    }

    #endregion

    #region Floor gate

    [Fact]
    public async Task Handle_NoFloorConfigured_FailsWithFloorMissing()
    {
        SetupFloor(new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None });

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationFloorMissing, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_OfferBelowFloor_FailsWithBelowFloor()
    {
        command.ProposedTotalRate = 1900m;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_OfferAtFloor_Sends()
    {
        command.ProposedTotalRate = 2000m;

        var result = await sut.Handle(command, CancellationToken.None);

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
        command.ProposedRatePerMile = null;

        var result = await sut.Handle(command, CancellationToken.None);

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

        var result = await sut.Handle(command, CancellationToken.None);

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

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await floorResolver.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default);
    }

    [Fact]
    public async Task Handle_LaterRoundBelowThreadSnapshot_FailsWithBelowFloor()
    {
        ExistingThread(floorTotal: 2500m);

        var result = await sut.Handle(command, CancellationToken.None);

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

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await negotiationRepo.Received(1).AddAsync(
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

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("rounds", result.Error, StringComparison.OrdinalIgnoreCase);
        await AssertNothingSent();
    }

    [Fact]
    public async Task Handle_EmailSendFails_PersistsNothing()
    {
        emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await negotiationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailSendFails_RevokesTheRouteItOpened()
    {
        emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await routeRegistry.Received(1).RevokeAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FirstOffer_OpensTheReplyRouteBeforeSending()
    {
        await sut.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            routeRegistry.OpenAsync(
                Arg.Any<string>(), tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
            emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_FirstOffer_PersistsThreadMessageAndRoute()
    {
        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.RoundCount);
        Assert.Equal(RateNegotiationStatus.AwaitingBroker, result.Value.Status);

        await negotiationRepo.Received(1).AddAsync(Arg.Any<RateNegotiation>(), Arg.Any<CancellationToken>());
        await messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m =>
                m.Direction == NegotiationMessageDirection.Outbound &&
                m.ProviderMessageId == "resend-1" &&
                m.ProposedTotalRate!.Amount == 2200m),
            Arg.Any<CancellationToken>());
        await routeRegistry.Received(1).OpenAsync(
            Arg.Any<string>(), tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await broadcastService.Received(1).BroadcastNegotiationAsync(tenant.Id, Arg.Any<RateNegotiationDto>());
    }

    [Fact]
    public async Task Handle_FirstOffer_SendsToListingBrokerWithThreadReplyAddress()
    {
        await sut.Handle(command, CancellationToken.None);

        await emailSender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e =>
                e.To == "broker@example.com" &&
                e.ReplyTo.StartsWith("offer-") &&
                e.ReplyTo.EndsWith("@mail.test.com") &&
                e.InReplyToMessageId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SecondOffer_ChainsThreadHeadersAndReusesRoute()
    {
        var existing = ExistingThread(floorTotal: 2000m);
        var first = existing.AddOutboundMessage("first offer");
        first.ProviderMessageId = "resend-0";
        messageRepo.Query().Returns(new List<NegotiationMessage> { first }.BuildMock());

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await emailSender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e => e.InReplyToMessageId == "resend-0" && e.References == "resend-0"),
            Arg.Any<CancellationToken>());
        await negotiationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await routeRegistry.DidNotReceiveWithAnyArgs().OpenAsync(default!, default, default);
        await routeRegistry.Received(1).RefreshAsync(
            existing.ReplyToken, tenant.Id, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDecisionId_BackfillsNegotiationOnDecision()
    {
        var decision = new AgentDecision { Type = AgentDecisionType.Query };
        decisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        command.DecisionId = decision.Id;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value!.Id, decision.NegotiationId);
    }

    #endregion
}
