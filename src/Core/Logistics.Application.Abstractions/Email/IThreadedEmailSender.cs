namespace Logistics.Application.Abstractions.Email;

/// <summary>
/// Sends an email that must thread into an existing conversation (RFC 5322 In-Reply-To/References),
/// with a caller-controlled reply-to address rather than the sender's default. Distinct from
/// <see cref="IEmailSender"/>, which sends one-off transactional mail with no threading.
/// </summary>
public interface IThreadedEmailSender
{
    Task<ThreadedEmailResult> SendAsync(ThreadedEmail email, CancellationToken ct = default);

    /// <summary>
    /// Domain the sender replies from, for building per-thread reply addresses
    /// (e.g. <c>offer-{token}@{ReplyDomain}</c>). Lets Application-layer callers derive a reply
    /// address without reaching into Infrastructure's sender configuration.
    /// </summary>
    string ReplyDomain { get; }
}

/// <summary>
/// A null <c>ReplyTo</c> sends from the configured sender address with no per-thread reply route,
/// which is what a one-off transactional email needs.
/// </summary>
public record ThreadedEmail(
    string To,
    string Subject,
    string HtmlBody,
    string? ReplyTo,
    string? InReplyToMessageId = null,
    string? References = null);

public record ThreadedEmailResult(bool Success, string? ProviderMessageId);
