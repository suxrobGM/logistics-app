using Sentry;
using Xunit;

namespace Logistics.HostDefaults.Tests;

public class SentryEventScrubberTests
{
    #region Events

    [Fact]
    public void Scrub_RequestWithSecrets_RemovesBodyCookiesQueryAndAuthHeaders()
    {
        var sentryEvent = new SentryEvent();
        sentryEvent.Request.Data = "{\"password\":\"hunter2\"}";
        sentryEvent.Request.Cookies = "session=abc";
        sentryEvent.Request.QueryString = "?access_token=secret";
        sentryEvent.Request.Headers["authorization"] = "Bearer secret";
        sentryEvent.Request.Headers["Cookie"] = "session=abc";
        sentryEvent.Request.Headers["X-Tenant"] = "acme";

        var result = SentryEventScrubber.Scrub(sentryEvent);

        Assert.NotNull(result);
        Assert.Null(result.Request.Data);
        Assert.Null(result.Request.Cookies);
        Assert.Null(result.Request.QueryString);
        Assert.False(result.Request.Headers.ContainsKey("authorization"));
        Assert.False(result.Request.Headers.ContainsKey("Cookie"));
        Assert.Equal("acme", result.Request.Headers["X-Tenant"]);
    }

    [Fact]
    public void Scrub_UserWithIdentity_KeepsOnlyOpaqueId()
    {
        var sentryEvent = new SentryEvent
        {
            User = new SentryUser { Id = "user-1", Email = "jane@carrier.com", Username = "jane", IpAddress = "10.0.0.1" }
        };

        var result = SentryEventScrubber.Scrub(sentryEvent);

        Assert.NotNull(result);
        Assert.Equal("user-1", result.User.Id);
        Assert.Null(result.User.Email);
        Assert.Null(result.User.Username);
        Assert.Null(result.User.IpAddress);
    }

    [Fact]
    public void Scrub_EmailInMessageAndExtra_RedactsIt()
    {
        var sentryEvent = new SentryEvent
        {
            Message = new SentryMessage { Formatted = "Failed to send payroll email to jane@carrier.com" }
        };
        sentryEvent.SetExtra("Email", "jane@carrier.com");
        sentryEvent.SetExtra("LoadId", 42);

        var result = SentryEventScrubber.Scrub(sentryEvent);

        Assert.NotNull(result);
        Assert.Equal("Failed to send payroll email to [redacted]", result.Message!.Formatted);
        Assert.Equal("[redacted]", result.Extra["Email"]);
        Assert.Equal(42, result.Extra["LoadId"]);
    }

    [Fact]
    public void Scrub_SerilogRequestLogEvent_DropsIt()
    {
        var sentryEvent = new SentryEvent { Logger = "Serilog.AspNetCore.RequestLoggingMiddleware" };

        Assert.Null(SentryEventScrubber.Scrub(sentryEvent));
    }

    #endregion

    #region Breadcrumbs

    [Fact]
    public void Scrub_BreadcrumbWithEmail_RedactsMessage()
    {
        var breadcrumb = new Breadcrumb("Created demo request from jane@carrier.com at Acme", "default", category: "app");

        var result = SentryEventScrubber.Scrub(breadcrumb);

        Assert.Equal("Created demo request from [redacted] at Acme", result.Message);
        Assert.Equal("app", result.Category);
    }

    [Fact]
    public void Scrub_BreadcrumbWithoutEmail_ReturnsSameInstance()
    {
        var breadcrumb = new Breadcrumb("Synced 12 loads", "default");

        Assert.Same(breadcrumb, SentryEventScrubber.Scrub(breadcrumb));
    }

    #endregion
}
