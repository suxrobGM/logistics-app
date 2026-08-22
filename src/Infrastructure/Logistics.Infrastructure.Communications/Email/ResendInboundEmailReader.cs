using Logistics.Application.Abstractions.Email;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>
/// Reads a received email through Resend's Received Emails API. The pinned SDK (0.2.1) has no
/// receiving surface, so this calls the endpoint directly.
/// </summary>
internal sealed class ResendInboundEmailReader(
    HttpClient httpClient,
    ILogger<ResendInboundEmailReader> logger) : IInboundEmailReader
{
    private const string BaseUrl = "https://api.resend.com/emails/receiving/";

    public async Task<InboundEmail?> GetAsync(string providerEmailId, CancellationToken ct = default)
    {
        var payload = await httpClient.TryGetFromJsonAsync<ReceivedEmailResponse>(
            BaseUrl + providerEmailId,
            logger,
            $"Fetching received email {providerEmailId} from Resend",
            IntegrationJsonOptions.SnakeCase,
            ct);

        if (payload is null || string.IsNullOrWhiteSpace(payload.From))
        {
            logger.LogWarning("Resend returned an unusable body for received email {EmailId}", providerEmailId);
            return null;
        }

        return new InboundEmail(
            payload.Id ?? providerEmailId,
            payload.From,
            payload.To ?? [],
            payload.Subject,
            payload.Text,
            payload.Html,
            payload.MessageId);
    }

    private sealed record ReceivedEmailResponse(
        string? Id,
        string? From,
        List<string>? To,
        string? Subject,
        string? Text,
        string? Html,
        string? MessageId);
}
