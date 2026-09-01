using System.Linq.Expressions;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;
using Logistics.Application.Modules.Integrations.Webhooks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Webhooks;

public class ProcessResendWebhookHandlerTests
{
    private const string Token = "abcdefghijklmnopqrstuvwxyz234567";

    private readonly IInboundEmailWebhookVerifier verifier = Substitute.For<IInboundEmailWebhookVerifier>();
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IMediator mediator = Substitute.For<IMediator>();

    private readonly IWebhookEventTracker webhookEvents = Substitute.For<IWebhookEventTracker>();
    private readonly IMasterRepository<InboundEmailRoute, Guid> routeRepo =
        Substitute.For<IMasterRepository<InboundEmailRoute, Guid>>();

    private readonly Guid tenantId = Guid.NewGuid();
    private readonly ProcessResendWebhookHandler sut;

    public ProcessResendWebhookHandlerTests()
    {
        masterUow.Repository<InboundEmailRoute>().Returns(routeRepo);

        verifier.Verify(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(true);

        SetupAlreadyHandled(false);
        SetupRoute(new InboundEmailRoute { ThreadToken = Token, TenantId = tenantId });

        mediator.Send(Arg.Any<ProcessInboundNegotiationEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        sut = new ProcessResendWebhookHandler(
            verifier, masterUow, tenantUow, mediator, webhookEvents,
            NullLogger<ProcessResendWebhookHandler>.Instance);
    }

    private static string Body(
        string type = "email.received",
        string emailId = "email-1",
        string? receivedFor = "offer-" + Token + "@mail.test.com") =>
        $$"""
        {
          "type": "{{type}}",
          "data": {
            "email_id": "{{emailId}}",
            "from": "broker@example.com",
            "subject": "Re: Rate offer",
            "message_id": "<abc@example.com>",
            "to": ["{{receivedFor}}"],
            "received_for": ["{{receivedFor}}"]
          }
        }
        """;

    private ProcessResendWebhookCommand Command(string? body = null) => new()
    {
        RawBody = body ?? Body(),
        SvixId = "msg_1",
        SvixTimestamp = "1700000000",
        SvixSignature = "v1,sig"
    };

    private void SetupAlreadyHandled(bool handled) =>
        webhookEvents.WasAlreadyHandledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(handled);

    private void SetupRoute(InboundEmailRoute? route) =>
        routeRepo.GetAsync(Arg.Any<Expression<Func<InboundEmailRoute, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(route);

    private Task AssertNoInnerCommand() =>
        mediator.DidNotReceive().Send(
            Arg.Any<ProcessInboundNegotiationEmailCommand>(), Arg.Any<CancellationToken>());

    [Fact]
    public async Task Handle_BadSignature_FailsAsRejectedWithoutParsing()
    {
        verifier.Verify(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(false);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.WebhookRejected, result.ErrorCode);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_UnparseableBody_FailsAsRejected()
    {
        var result = await sut.Handle(Command("not json"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.WebhookRejected, result.ErrorCode);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_UnhandledEventType_IsAccepted()
    {
        var result = await sut.Handle(Command(Body(type: "email.delivered")), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_Replay_IsAcceptedAndDoesNothing()
    {
        SetupAlreadyHandled(true);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await AssertNoInnerCommand();
        await webhookEvents.DidNotReceiveWithAnyArgs().MarkHandledAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_AddressWithoutThreadToken_IsAccepted()
    {
        var result = await sut.Handle(
            Command(Body(receivedFor: "hello@mail.test.com")), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_UnknownToken_IsAcceptedWithoutOpeningATenant()
    {
        SetupRoute(null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await tenantUow.DidNotReceiveWithAnyArgs().SetCurrentTenantByIdAsync(default);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_RevokedRoute_IsAccepted()
    {
        SetupRoute(new InboundEmailRoute
        {
            ThreadToken = Token,
            TenantId = tenantId,
            RevokedAt = DateTime.UtcNow
        });

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_RouteFromLapsedThread_IsAcceptedWithoutProcessing()
    {
        SetupRoute(new InboundEmailRoute
        {
            ThreadToken = Token,
            TenantId = tenantId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await AssertNoInnerCommand();
    }

    [Fact]
    public async Task Handle_InnerCommandFails_FailsWithoutRejectedCodeAndWritesNoLedgerRow()
    {
        mediator.Send(Arg.Any<ProcessInboundNegotiationEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("provider unavailable"));

        var result = await sut.Handle(Command(), CancellationToken.None);

        // No rejection code: the endpoint must answer 500 so the provider retries the delivery.
        Assert.False(result.IsSuccess);
        Assert.Null(result.ErrorCode);
        await webhookEvents.DidNotReceiveWithAnyArgs().MarkHandledAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_HappyPath_RoutesToTenantAndRecordsTheEvent()
    {
        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(tenantId);
        await mediator.Received(1).Send(
            Arg.Is<ProcessInboundNegotiationEmailCommand>(c =>
                c.ThreadToken == Token &&
                c.ProviderEmailId == "email-1" &&
                c.From == "broker@example.com"),
            Arg.Any<CancellationToken>());
        await webhookEvents.Received(1).MarkHandledAsync(
            "Resend",
            "email-1",
            Arg.Any<CancellationToken>());
    }
}
