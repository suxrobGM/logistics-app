using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;
using Logistics.Domain.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Webhooks;

public class ForwardInboundEmailHandlerTests
{
    private const string Owner = "owner@example.com";

    private readonly IInboundEmailReader _reader = Substitute.For<IInboundEmailReader>();
    private readonly IThreadedEmailSender _sender = Substitute.For<IThreadedEmailSender>();
    private readonly OwnerNotificationOptions _options = new() { Email = Owner };
    private readonly ForwardInboundEmailHandler _sut;

    public ForwardInboundEmailHandlerTests()
    {
        _sender.ReplyDomain.Returns("logisticsx.app");
        _sender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(true, "sent-1"));
        SetupEmail("rogelio@example.com", html: null, text: "Hi team, <questions>");

        _sut = new ForwardInboundEmailHandler(
            _reader, _sender, Options.Create(_options), NullLogger<ForwardInboundEmailHandler>.Instance);
    }

    private static ForwardInboundEmailCommand Command() => new() { ProviderEmailId = "email-1" };

    private void SetupEmail(string from, string? html, string? text, string? subject = "Evaluating LogisticsX") =>
        _reader.GetAsync("email-1", Arg.Any<CancellationToken>())
            .Returns(new InboundEmail("email-1", from, ["sukhrob@logisticsx.app"], subject, text, html, "<m@x>"));

    [Fact]
    public async Task Handle_ForwardsToOwnerWithReplyToTheSender()
    {
        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _sender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e =>
                e.To == Owner &&
                e.ReplyTo == "rogelio@example.com" &&
                e.Subject == "Evaluating LogisticsX" &&
                e.HtmlBody.Contains("To: sukhrob@logisticsx.app") &&
                e.HtmlBody.Contains("Hi team, &lt;questions&gt;")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HtmlBody_IsForwardedAsIs()
    {
        SetupEmail("rogelio@example.com", html: "<p>Hello</p>", text: "Hello");

        await _sut.Handle(Command(), CancellationToken.None);

        await _sender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e => e.HtmlBody.EndsWith("<p>Hello</p>")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoRecipientConfigured_DoesNothing()
    {
        _options.Email = null;

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _reader.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
    }

    [Fact]
    public async Task Handle_MailFromOwnDomain_IsNotForwarded()
    {
        SetupEmail("noreply@LogisticsX.app", html: null, text: "loop");

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _sender.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task Handle_EmailNotReadable_Fails()
    {
        _reader.GetAsync("email-1", Arg.Any<CancellationToken>()).Returns((InboundEmail?)null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_SendFails_Fails()
    {
        _sender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
