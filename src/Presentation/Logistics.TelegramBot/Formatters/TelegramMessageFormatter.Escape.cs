using System.Text.RegularExpressions;

namespace Logistics.TelegramBot.Formatters;

internal static partial class TelegramMessageFormatter
{
    /// <summary>
    /// Escapes special characters for Telegram MarkdownV2 format.
    /// </summary>
    public static string Escape(string text)
    {
        return MarkdownV2EscapeRegex().Replace(text, @"\$0");
    }

    [GeneratedRegex(@"([_\*\[\]\(\)~`>#+\-=|{}\.\!\\])")]
    private static partial Regex MarkdownV2EscapeRegex();
}
