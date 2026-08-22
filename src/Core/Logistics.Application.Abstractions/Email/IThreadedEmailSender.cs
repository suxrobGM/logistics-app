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
/// which is what a one-off transactional email needs. <c>MessageId</c> is the RFC 5322 Message-ID to
/// send under, angle brackets included; null lets the provider assign one, which only a mail that
/// nothing will ever reply into can afford.
/// </summary>
public record ThreadedEmail(
    string To,
    string Subject,
    string HtmlBody,
    string? ReplyTo,
    string? MessageId = null,
    string? InReplyToMessageId = null,
    string? References = null);

/// <param name="ProviderMessageId">
/// The provider's own handle on the send, for support lookups. Not an RFC 5322 Message-ID and never
/// usable as one - threading headers must quote <see cref="ThreadedEmail.MessageId"/>.
/// </param>
public record ThreadedEmailResult(bool Success, string? ProviderMessageId);
