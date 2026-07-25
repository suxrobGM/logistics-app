namespace Logistics.Domain.Entities;

/// <summary>
/// The size and shape rules for a dispatch policy document, in one place.
/// <para>
/// Lives in Domain because every layer that enforces them references it: the Application learner,
/// the Infrastructure.AI prompt builder, the persistence configuration, and the update validator.
/// </para>
/// </summary>
public static class DispatchPolicyText
{
    /// <summary>
    /// Maximum characters of policy text handed to the agent (~1k tokens). Both the learned and the
    /// dispatcher-authored sections are held to this budget between them.
    /// </summary>
    public const int MaxContentChars = 4000;

    /// <summary>
    /// Storage headroom: a pass may be stored slightly over budget and clamped at the prompt
    /// boundary, so the column is deliberately wider than <see cref="MaxContentChars"/>.
    /// </summary>
    public const int MaxStoredChars = MaxContentChars * 2;

    /// <summary>
    /// Clamps <paramref name="text"/> to <paramref name="maxChars"/> keeping only whole lines.
    /// <para>
    /// Returns null when not even the first line fits: half a rule reads as a different rule, so a
    /// fragment is worse than nothing. Only use this on multi-line documents - single-line fields
    /// want a plain hard cut instead.
    /// </para>
    /// </summary>
    public static string? KeepWholeLinesWithin(string? text, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(text) || maxChars <= 0)
        {
            return null;
        }

        var trimmed = text.Trim();
        if (trimmed.Length <= maxChars)
        {
            return trimmed;
        }

        var lastBreak = trimmed[..maxChars].LastIndexOf('\n');
        if (lastBreak <= 0)
        {
            return null;
        }

        var result = trimmed[..lastBreak].TrimEnd();
        return result.Length == 0 ? null : result;
    }
}
