using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class EventNotifierTests
{
    private readonly IPushNotificationService _push = Substitute.For<IPushNotificationService>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IEmailTemplateService _templates = Substitute.For<IEmailTemplateService>();
    private readonly EventNotifier _sut;

    public EventNotifierTests()
    {
        _sut = new EventNotifier(_push, Substitute.For<INotificationService>(),
            Substitute.For<ITelegramNotificationService>(), _email, _templates,
            Substitute.For<ITenantUnitOfWork>(), NullLogger<EventNotifier>.Instance);
    }

    [Fact]
    public async Task PushAsync_SkipsMissingTokensAndSendsEachTokenOnce()
    {
        await _sut.PushAsync("Title", "Body", null, "a", null, "", "a", "b");

        await _push.Received(1).SendNotificationAsync("Title", "Body", "a", null);
        await _push.Received(1).SendNotificationAsync("Title", "Body", "b", null);
        await _push.Received(2).SendNotificationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>());
    }

    /// <summary>The change behind the event is saved already; one dead device must not stop the next one.</summary>
    [Fact]
    public async Task PushAsync_OneTokenFails_StillSendsTheOthers()
    {
        _push.SendNotificationAsync(Arg.Any<string>(), Arg.Any<string>(), "a", Arg.Any<IReadOnlyDictionary<string, string>?>())
            .ThrowsAsync(new InvalidOperationException("unregistered"));

        await _sut.PushAsync("Title", "Body", null, "a", "b");

        await _push.Received(1).SendNotificationAsync("Title", "Body", "b", null);
    }

    [Fact]
    public async Task EmailAsync_NoAddress_SendsNothing()
    {
        await _sut.EmailAsync(null, "Subject", "Template", new object());

        await _email.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
