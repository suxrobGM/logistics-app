using System.Text.RegularExpressions;
using Sentry;

namespace Logistics.HostDefaults;

/// <summary>
/// Strips personal data and credentials from Sentry events and breadcrumbs before they leave the
/// process: request bodies, cookies, auth headers, the query string (SignalR carries the access
/// token there), user identity beyond the opaque id, and any email address in a message or log
/// property.
/// </summary>
public static partial class SentryEventScrubber
{
    /// <summary>
    /// The Serilog request-log line duplicates the exception event the error handler already
    /// logged, and carries the response body and query string, so it is dropped.
    /// </summary>
    private const string RequestLoggingSource = "Serilog.AspNetCore.RequestLoggingMiddleware";

    private const string Redacted = "[redacted]";

    private static readonly string[] SensitiveHeaders =
        ["Authorization", "Cookie", "Set-Cookie", "Proxy-Authorization", "X-Api-Key", "Stripe-Signature"];

    public static SentryEvent? Scrub(SentryEvent sentryEvent)
    {
        if (sentryEvent.Logger == RequestLoggingSource)
        {
            return null;
        }

        var request = sentryEvent.Request;
        request.Data = null;
        request.Cookies = null;
        request.QueryString = null;

        foreach (var header in SensitiveHeaders)
        {
            var key = request.Headers.Keys.FirstOrDefault(k => k.Equals(header, StringComparison.OrdinalIgnoreCase));
            if (key is not null)
            {
                request.Headers.Remove(key);
            }
        }

        sentryEvent.User.Email = null;
        sentryEvent.User.Username = null;
        sentryEvent.User.IpAddress = null;

        if (sentryEvent.Message is { } message)
        {
            message.Formatted = message.Formatted is { } formatted ? RedactEmails(formatted) : null;
        }

        foreach (var (key, value) in sentryEvent.Extra.ToList())
        {
            if (value?.ToString() is { } text && EmailPattern().IsMatch(text))
            {
                sentryEvent.SetExtra(key, RedactEmails(text));
            }
        }

        return sentryEvent;
    }

    public static Breadcrumb Scrub(Breadcrumb breadcrumb)
    {
        if (breadcrumb.Message is null || !EmailPattern().IsMatch(breadcrumb.Message))
        {
            return breadcrumb;
        }

        return new Breadcrumb(
            RedactEmails(breadcrumb.Message),
            breadcrumb.Type ?? "default",
            breadcrumb.Data,
            breadcrumb.Category,
            breadcrumb.Level);
    }

    private static string RedactEmails(string text) => EmailPattern().Replace(text, Redacted);

    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
    private static partial Regex EmailPattern();
}
