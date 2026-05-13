using System.Text.Json;

namespace Logistics.Infrastructure.Integrations.Eld.Common;

/// <summary>
/// Shared <see cref="JsonSerializerOptions"/> instances for ELD provider APIs. Each provider
/// has a consistent JSON casing convention; using a naming policy avoids decorating every
/// model property with <c>[JsonPropertyName]</c>.
/// </summary>
internal static class EldJsonOptions
{
    /// <summary>
    /// camelCase wire format. Used by Samsara, Geotab, and most TT ELD endpoints.
    /// </summary>
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// snake_case wire format. Used by Motive (KeepTruckin).
    /// </summary>
    public static readonly JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
}
