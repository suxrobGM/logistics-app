using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Sanitisers and rules shared by both agents' prompt builders. Separate from either prompt class so that
/// neither has to reach into the other for a string helper.
/// </summary>
internal static class PromptText
{
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

    /// <summary>The unit rule both prompts state; tools that still report km need converting for a miles tenant.</summary>
    public static string DistanceKeys(DistanceUnit unit) =>
        unit == DistanceUnit.Kilometers
            ? "Every distance key ends in its unit (`_km`, `_mi`)."
            : "Every distance key ends in its unit (`_km`, `_mi`). Convert `_km` values to miles (× 0.621).";
}
