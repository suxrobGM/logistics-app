using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

/// <summary>
/// Sends a load board request and throws <see cref="LoadBoardApiException"/> with the vendor's
/// own error text on any failure. Deliberately not the never-throwing
/// <c>Integrations.Common</c> helpers: a swallowed failure reads as "no freight" to a dispatcher.
/// </summary>
internal static class LoadBoardHttp
{
    private const int MaxErrorBodyLength = 300;

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Sends <paramref name="body"/> as JSON (when not null) with an optional bearer token and
    /// reads the response body as <typeparamref name="T"/>.
    /// </summary>
    public static async Task<T> SendJsonAsync<T>(
        this HttpClient client,
        HttpMethod method,
        string url,
        object? body,
        string? bearerToken,
        string vendor,
        CancellationToken ct)
    {
        using var response = await SendCoreAsync(client, method, url, body, bearerToken, vendor, ct);
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(Json, ct)
                   ?? throw new LoadBoardApiException($"{vendor} sent an empty response.", response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new LoadBoardApiException($"{vendor} sent a response we could not read: {ex.Message}", response.StatusCode);
        }
    }

    /// <summary>
    /// Sends a request whose response body is not needed.
    /// </summary>
    public static async Task SendJsonAsync(
        this HttpClient client,
        HttpMethod method,
        string url,
        object? body,
        string? bearerToken,
        string vendor,
        CancellationToken ct)
    {
        using var response = await SendCoreAsync(client, method, url, body, bearerToken, vendor, ct);
    }

    private static async Task<HttpResponseMessage> SendCoreAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        object? body,
        string? bearerToken,
        string vendor,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);
        if (bearerToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new LoadBoardApiException($"Could not reach {vendor}: {ex.Message}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new LoadBoardApiException($"{vendor} did not answer in time.");
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        using (response)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new LoadBoardApiException(Describe(vendor, response.StatusCode, error), response.StatusCode);
        }
    }

    private static string Describe(string vendor, HttpStatusCode status, string body)
    {
        var detail = string.IsNullOrWhiteSpace(body)
            ? string.Empty
            : $" - {(body.Length > MaxErrorBodyLength ? body[..MaxErrorBodyLength] + "..." : body)}";

        var summary = status switch
        {
            HttpStatusCode.Unauthorized => $"{vendor} rejected the credentials (401)",
            HttpStatusCode.Forbidden => $"{vendor} refused access (403); check the account includes API access",
            HttpStatusCode.TooManyRequests => $"{vendor} rate limit reached (429); try again in a minute",
            _ => $"{vendor} returned {(int)status} {status}"
        };

        return summary + detail;
    }
}
