namespace Logistics.Application.Modules.Integrations.Negotiation;

public static class NegotiationText
{
    /// <summary>
    /// Cuts <paramref name="text"/> to <paramref name="maxLength"/>. With
    /// <paramref name="wordBoundary"/> the cut backs up to the last space so a broker-facing excerpt
    /// never ends mid-word; without it the cut is exact, which is what storage limits need.
    /// </summary>
    public static string Truncate(
        string text, int maxLength, string ellipsis = "...", bool wordBoundary = false)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var truncated = text[..maxLength];
        var lastSpace = truncated.LastIndexOf(' ');

        if (wordBoundary && lastSpace > 0)
        {
            truncated = truncated[..lastSpace];
        }

        return truncated.TrimEnd() + ellipsis;
    }
}
