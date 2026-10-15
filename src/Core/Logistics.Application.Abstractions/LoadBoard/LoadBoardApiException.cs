using System.Net;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// A load board vendor call failed. Providers throw it with the vendor's own error text so the
/// failure reaches the dispatcher per provider instead of turning into an empty result.
/// </summary>
public sealed class LoadBoardApiException(string message, HttpStatusCode? statusCode = null)
    : Exception(message)
{
    /// <summary>The vendor's HTTP status, or null when the request never completed.</summary>
    public HttpStatusCode? StatusCode { get; } = statusCode;

    /// <summary>The vendor rejected the access token; the gateway re-authenticates once and retries.</summary>
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}
