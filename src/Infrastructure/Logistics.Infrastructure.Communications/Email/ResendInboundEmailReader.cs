using System.Net.Http.Headers;
using System.Text.Json;
using Logistics.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>
/// Reads a received email through Resend's Received Emails API. The pinned SDK (0.2.1) has no
/// receiving surface, so this calls the endpoint directly.
/// </summary>
internal sealed class ResendInboundEmailReader(
    HttpClient httpClient,
    IOptions<ResendOptions> options,
    ILogger<ResendInboundEmailReader> logger) : IInboundEmailReader
{
    private const string BaseUrl = "https://api.resend.com/emails/receiving/";

    public async Task<InboundEmail?> GetAsync(string providerEmailId, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + providerEmailId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

            using var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Resend returned {Status} fetching received email {EmailId}",
                    (int)response.StatusCode, providerEmailId);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<ReceivedEmailResponse>(stream, JsonOptions, ct);

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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch received email {EmailId} from Resend", providerEmailId);
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private sealed record ReceivedEmailResponse(
        string? Id,
        string? From,
        List<string>? To,
        string? Subject,
        string? Text,
        string? Html,
        string? MessageId);
}
