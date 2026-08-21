using System.Text;
using System.Text.RegularExpressions;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Strips quoted history and signatures from a reply, leaving what the broker actually typed.
/// Heuristic by nature: a miss costs the agent some noise, so every rule errs toward keeping text.
/// </summary>
internal static partial class EmailReplyParser
{
    private const int MaxLength = 8000;

    /// <summary>"On Mon, 3 Mar 2026 at 09:12, Someone &lt;a@b.com&gt; wrote:" and its variants.</summary>
    [GeneratedRegex(@"^\s*On\s.+\bwrote:\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex OnWroteLine();

    [GeneratedRegex(@"^\s*-{2,}\s*(Original Message|Forwarded message)\s*-{2,}\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex OriginalMessageLine();

    [GeneratedRegex(@"^\s*(From|Sent|To|Subject):\s", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderBlockLine();

    [GeneratedRegex(@"^\s*(Sent from my |Get Outlook for )", RegexOptions.IgnoreCase)]
    private static partial Regex MobileFooterLine();

    public static string Strip(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "";
        }

        var kept = new StringBuilder();

        foreach (var line in body.ReplaceLineEndings("\n").Split('\n'))
        {
            if (IsCutPoint(line))
            {
                break;
            }

            if (line.TrimStart().StartsWith('>'))
            {
                continue;
            }

            kept.Append(line).Append('\n');
        }

        var text = kept.ToString().Trim();

        // Everything quoted: the reply is probably a bare forward, so the original body beats nothing.
        if (text.Length == 0)
        {
            text = body.Trim();
        }

        return text.Length <= MaxLength ? text : text[..MaxLength].TrimEnd() + "...";
    }

    private static bool IsCutPoint(string line) =>
        OnWroteLine().IsMatch(line) ||
        OriginalMessageLine().IsMatch(line) ||
        HeaderBlockLine().IsMatch(line) ||
        MobileFooterLine().IsMatch(line) ||
        line.TrimEnd() == "--";
}
