using System.Globalization;
using System.Text.Json.Nodes;

namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Lenient accessors for reading a persisted <c>AgentDecision.ToolInput</c> back. Tools do not need
/// these - their arguments arrive typed - and every accessor returns null rather than throwing,
/// because the callers here are recording metadata, not validating a call.
/// </summary>
public static class ToolInput
{
    extension(JsonNode input)
    {
        public string? GetString(string key) =>
            input[key] is JsonValue value ? value.ToString() : null;

        public Guid? GetGuid(string key) =>
            Guid.TryParse(input.GetString(key), out var guid) ? guid : null;

        /// <summary>
        /// Invariant, because the JSON was written invariant: under a comma-decimal culture the ambient
        /// parse reads "1234.56" as 123456 and the recorded rate is a hundred times the real one.
        /// </summary>
        public decimal? GetDecimal(string key) =>
            decimal.TryParse(input.GetString(key), NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
    }

}
