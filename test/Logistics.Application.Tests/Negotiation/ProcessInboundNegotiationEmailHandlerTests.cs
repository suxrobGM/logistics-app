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
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly IInboundEmailReader inboundEmailReader = Substitute.For<IInboundEmailReader>();
    private readonly INegotiationTurnStarter turnStarter = Substitute.For<INegotiationTurnStarter>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<RateNegotiation, Guid> negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<NegotiationMessage, Guid> messageRepo =
        Substitute.For<ITenantRepository<NegotiationMessage, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();

    private readonly Tenant tenant;
    private readonly RateNegotiation negotiation;
    private readonly ProcessInboundNegotiationEmailHandler sut;

    public ProcessInboundNegotiationEmailHandlerTests()
    {
        tenant = TestTenant.Create();

        negotiation = RateNegotiation.Create(
            Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None, conversationId: Guid.NewGuid());
        negotiation.AddOutboundMessage("first offer");

        tenantUow.Repository<RateNegotiation>().Returns(negotiationRepo);
        tenantUow.Repository<NegotiationMessage>().Returns(messageRepo);
        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);

        featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AIRateNegotiation).Returns(true);

        negotiationRepo.GetAsync(Arg.Any<Expression<Func<RateNegotiation, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(negotiation);

        inboundEmailReader.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new InboundEmail(
                "email-1",
                "Pat Broker <broker@example.com>",
                ["offer-token@mail.test.com"],
                "Re: Rate offer",
                "We can do 2100.\n\nOn Mon, Dispatch <d@c.com> wrote:\n> 2200 all in",
                null,
                "<reply@example.com>"));

        sut = new ProcessInboundNegotiationEmailHandler(
            tenantUow, featureService, inboundEmailReader, turnStarter, broadcastService,
            NullLogger<ProcessInboundNegotiationEmailHandler>.Instance);
    }

    private ProcessInboundNegotiationEmailCommand Command(string from = "Pat Broker <broker@example.com>") => new()
    {
        ThreadToken = negotiation.ReplyToken,
        ProviderEmailId = "email-1",
        From = from
    };

    private Task AssertAgentNotWoken() =>
        turnStarter.DidNotReceiveWithAnyArgs().NotifyBrokerReplyAsync(default!, default!, default);

    [Fact]
    public async Task Handle_FeatureDisabled_DropsTheReply()
    {
        featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AIRateNegotiation).Returns(false);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_ClosedThread_IgnoresTheReply()
    {
        negotiation.Close(RateNegotiationStatus.Closed);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_BodyFetchFails_ReturnsRetryableFailure()
    {
        inboundEmailReader.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((InboundEmail?)null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await messageRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SenderIsNotTheBroker_QuarantinesAndNeverWakesTheAgent()
    {
        var result = await sut.Handle(Command("attacker@evil.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.AwaitingBroker, negotiation.Status);
        await messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m => m.Quarantined && m.TextBody == ""),
            Arg.Any<CancellationToken>());
        await AssertAgentNotWoken();
    }

    [Fact]
    public async Task Handle_BrokerReply_StoresStrippedTextAndWakesTheAgent()
    {
        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.BrokerReplied, negotiation.Status);
        await messageRepo.Received(1).AddAsync(
            Arg.Is<NegotiationMessage>(m =>
                !m.Quarantined &&
                m.Direction == NegotiationMessageDirection.Inbound &&
                m.TextBody == "We can do 2100." &&
                m.RawBody!.Contains("2200 all in")),
            Arg.Any<CancellationToken>());
        await turnStarter.Received(1).NotifyBrokerReplyAsync(
            negotiation, "We can do 2100.", Arg.Any<CancellationToken>());
        await broadcastService.Received(1).BroadcastNegotiationAsync(
            tenant.Id, Arg.Any<Logistics.Shared.Models.RateNegotiationDto>());
    }
}
