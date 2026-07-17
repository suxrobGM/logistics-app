using System.Net;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Outcome of a write-side HTTP call made through <see cref="HttpClientWriteExtensions"/>:
/// success flag, deserialised value (when successful), error body, and the HTTP status code
/// (null when the request never completed).
/// </summary>
public sealed record HttpJsonResult<T>(bool IsSuccess, T? Value, string ErrorBody, HttpStatusCode? StatusCode);
