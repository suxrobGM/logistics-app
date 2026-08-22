namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// <c>SanitizedMessage</c> is the agent paragraph after sanitization, so the caller can store what
/// the broker actually reads instead of re-running the same cleanup.
/// </summary>
public record ComposedNegotiationEmail(
    string Subject,
    string HtmlBody,
    string SanitizedMessage);
