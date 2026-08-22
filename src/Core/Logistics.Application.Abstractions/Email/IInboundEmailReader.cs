namespace Logistics.Application.Abstractions.Email;

/// <summary>
/// Fetches the body of an email the provider received for us. Inbound webhooks carry metadata
/// only, so the body is always a second call.
/// </summary>
public interface IInboundEmailReader
{
    /// <summary>Returns null when the provider does not answer - the caller should retry later.</summary>
    Task<InboundEmail?> GetAsync(string providerEmailId, CancellationToken ct = default);
}

public record InboundEmail(
    string Id,
    string From,
    IReadOnlyList<string> To,
    string? Subject,
    string? TextBody,
    string? HtmlBody,
    string? MessageId);
