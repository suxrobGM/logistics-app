using System.Linq.Expressions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class ProcessInboundNegotiationEmailHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFeatureService _featureService = Substitute.For<IFeatureService>();
    private readonly IInboundEmailReader _inboundEmailReader = Substitute.For<IInboundEmailReader>();
    private readonly INegotiationTurnStarter _turnStarter = Substitute.For<INegotiationTurnStarter>();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<RateNegotiation, Guid> _negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<NegotiationMessage, Guid> _messageRepo =
        Substitute.For<ITenantRepository<NegotiationMessage, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> _listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();

    private readonly Tenant _tenant;
    private readonly RateNegotiation _negotiation;
    private readonly ProcessInboundNegotiationEmailHandler _sut;

    public ProcessInboundNegotiationEmailHandlerTests()
    {
        _tenant = TestTenant.Create();

        _negotiation = RateNegotiation.Create(
            Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None, conversationId: Guid.NewGuid());
        _negotiation.AddOutboundMessage("first offer");

        _tenantUow.Repository<RateNegotiation>().Returns(_negotiationRepo);
        _tenantUow.Repository<NegotiationMessage>().Returns(_messageRepo);
        _tenantUow.Repository<LoadBoardListing>().Returns(_listingRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);

        _featureService.IsFeatureEnabledAsync(_tenant.Id, TenantFeature.AIRateNegotiation).Returns(true);

        _negotiationRepo.GetAsync(Arg.Any<Expression<Func<RateNegotiation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_negotiation);

        _inboundEmailReader.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new InboundEmail(
                "email-1",
                "Pat Broker <broker@example.com>",
                ["offer-token@mail.test.com"],
                "Re: Rate offer",
                "We can do 2100.\n\nOn Mon, Dispatch <d@c.com> wrote:\n> 2200 all in",
                null,
                "<reply@example.com>"));

        _sut = new ProcessInboundNegotiationEmailHandler(
            _tenantUow, _featureService, _inboundEmailReader, _turnStarter, _broadcastService,
            NullLogger<ProcessInboundNegotiationEmailHandler>.Instance);
    }

    private ProcessInboundNegotiationEmailCommand Command(string from = "Pat Broker <broker@example.com>") => new()
    {
        ThreadToken = _negotiation.ReplyToken,
        ProviderEmailId = "email-1",
        From = from
    };

    private Task AssertAgentNotWoken() =>
        _turnStarter.DidNotReceiveWithAnyArgs().NotifyBrokerReplyAsync(default!, default!, default);

    [Fact]
    public async Task Handle_FeatureDisabled_DropsTheReply()
    {
        _featureService.IsFeatureEnabledAsync(_tenant.Id, TenantFeature.AIRateNegotiation).Returns(false);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_ClosedThread_IgnoresTheReply()
    {
        _negotiation.Close(RateNegotiationStatus.Closed);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_BodyFetchFails_ReturnsRetryableFailure()
    {
        _inboundEmailReader.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((InboundEmail?)null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SenderIsNotTheBroker_QuarantinesAndNeverWakesTheAgent()
    {
        var result = await _sut.Handle(Command("attacker@evil.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.AwaitingBroker, _negotiation.Status);
        await _messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m => m.Quarantined && m.TextBody == ""),
            Arg.Any<CancellationToken>());
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_BrokerReply_StoresStrippedTextAndWakesTheAgent()
    {
        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.BrokerReplied, _negotiation.Status);
        await _messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m =>
                !m.Quarantined &&
                m.Direction == NegotiationMessageDirection.Inbound &&
                m.TextBody == "We can do 2100." &&
                m.RawBody!.Contains("2200 all in")),
            Arg.Any<CancellationToken>());
        await _turnStarter.Received(1).NotifyBrokerReplyAsync(
            _negotiation, "We can do 2100.", Arg.Any<CancellationToken>());
        await _broadcastService.Received(1).BroadcastNegotiationAsync(
            _tenant.Id, Arg.Any<Logistics.Shared.Models.RateNegotiationDto>());
    }
}
