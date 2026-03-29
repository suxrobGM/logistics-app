namespace Logistics.TelegramBot.Options;

internal sealed class TelegramBotOptions
{
    public const string SectionName = "TelegramBot";
    public string BotToken { get; set; } = "";
    public string? WebhookUrl { get; set; }
    public string? SecretToken { get; set; }
}
