using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Accounting.Common;

/// <summary>
///     Shared JSON options for the QuickBooks Online API. QBO uses PascalCase property names
///     (e.g. "DisplayName", "TotalAmt"), so no naming policy is applied; reads are
///     case-insensitive and nulls are omitted on write.
/// </summary>
internal static class QboJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
