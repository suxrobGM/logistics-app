using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Sanitisers and rule text shared by both agents' prompt builders. Separate from either prompt
/// class so that neither has to reach into the other for a string helper.
/// </summary>
internal static class PromptText
{
    /// <summary>
    /// The hard truck/load pairings as markdown bullets, rendered from
    /// <see cref="TruckLoadCompatibility"/> so the prompt names the enum values the tools return.
    /// </summary>
    public static string TruckTypeRules { get; } = string.Join('\n',
        Rule(TruckLoadCompatibility.VehicleLoadTypes, TruckLoadCompatibility.VehicleTruckTypes),
        Rule(TruckLoadCompatibility.ContainerLoadTypes, TruckLoadCompatibility.ContainerTruckTypes));

    /// <summary>
    /// The shared prompt-injection defence for every untrusted value a prompt interpolates.
    /// </summary>
    /// <param name="allowLineBreaks">
    /// True for multi-line documents, where newlines and tabs carry the markdown structure. False for
    /// single-line values like a company name, where a newline could forge a new prompt section.
    /// </param>
    public static string StripControlChars(string text, bool allowLineBreaks) =>
        new([.. text.Where(c => !char.IsControl(c) || (allowLineBreaks && c is '\n' or '\t'))]);

    public static string SanitizeCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Fleet";

        var sanitized = StripControlChars(name, allowLineBreaks: false);
        return sanitized.Length > 100 ? sanitized[..100] : sanitized;
    }

    private static string Rule(IEnumerable<LoadType> loads, IEnumerable<TruckType> trucks) =>
        $"- {Ticked(loads, ", ")} loads → {Ticked(trucks, " or ")} only, and those trucks carry nothing else";

    private static string Ticked<T>(IEnumerable<T> values, string separator) =>
        string.Join(separator, values.Select(v => $"`{v}`"));
}
