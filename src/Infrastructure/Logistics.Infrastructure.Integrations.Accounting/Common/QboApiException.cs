using System.Net;
using System.Text.Json;
using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

namespace Logistics.Infrastructure.Integrations.Accounting.Common;

/// <summary>
///     Raised when a QuickBooks Online API call returns a non-success status. The message carries QBO's
///     fault and the <c>intuit_tid</c> transaction id Intuit support asks for, so the sync job can record
///     it against the failed entity mapping as-is.
/// </summary>
internal sealed class QboApiException(string action, HttpStatusCode statusCode, string? body, string? intuitTid)
    : Exception(BuildMessage(action, statusCode, body, intuitTid))
{
    /// <summary>QBO "Stale Object Error": the SyncToken sent is older than the record's current one.</summary>
    private const string StaleObjectCode = "5010";

    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? IntuitTid { get; } = intuitTid;
    public IReadOnlyList<string> ErrorCodes { get; } = ParseFault(body)?.Error?.Select(e => e.Code ?? "").ToList() ?? [];

    public bool IsStaleObject => ErrorCodes.Contains(StaleObjectCode);

    private static string BuildMessage(string action, HttpStatusCode statusCode, string? body, string? intuitTid)
    {
        var errors = ParseFault(body)?.Error;
        var detail = errors is { Count: > 0 }
            ? string.Join("; ", errors.Select(e => $"{e.Message}: {e.Detail} (code {e.Code})"))
            : Truncate(body);
        var tid = string.IsNullOrEmpty(intuitTid) ? "" : $" [intuit_tid {intuitTid}]";
        return $"QBO {action} failed with {(int)statusCode} {statusCode}: {detail}{tid}";
    }

    private static QboFault? ParseFault(string? body)
    {
        if (string.IsNullOrWhiteSpace(body) || body[0] != '{')
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<QboFaultResponse>(body, QboJsonOptions.Default)?.Fault;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Truncate(string? body) =>
        string.IsNullOrEmpty(body) ? "(no body)" :
        body.Length <= 1000 ? body : body[..1000];
}
