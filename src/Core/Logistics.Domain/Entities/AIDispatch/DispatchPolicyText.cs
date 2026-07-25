namespace Logistics.Domain.Entities;

/// <summary>
/// Size and shape rules for a dispatch policy document. In Domain because every layer that enforces
/// them references it: the learner, the prompt builder, the persistence config and the update validator.
/// </summary>
public static class DispatchPolicyText
{
    /// <summary>
    /// Maximum characters handed to the agent (~1k tokens), shared between the learned and the
    /// dispatcher-authored sections.
    /// </summary>
    public const int MaxContentChars = 4000;

    /// <summary>
    /// Storage headroom: a pass may be stored over budget and clamped at the prompt boundary, so the
    /// column is deliberately wider than <see cref="MaxContentChars"/>.
    /// </summary>
    public const int MaxStoredChars = MaxContentChars * 2;

    /// <summary>
    /// Clamps <paramref name="text"/> to <paramref name="maxChars"/> keeping whole lines only - half a
    /// rule reads as a different rule. Returns null when not even the first line fits. Multi-line
    /// documents only; single-line fields want a plain hard cut.
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
