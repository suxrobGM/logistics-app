using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Commands;

internal sealed class StartCommand : ITelegramCommand
{
    public string Name => "/start";
    public string Description => "Welcome message and setup instructions";
    public bool RequiresAuth => false;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        var text = context is not null
            ? $"""
              Welcome back\\! You are connected to *{TelegramMessageFormatter.Escape(context.TenantName)}*\\.

              Use /help to see available commands\\.
              """
            : """
              Welcome to *LogisticsX Bot*\\!

              To get started, connect your account using an API key:

              `/connect your_api_key_here`

              You can generate an API key in the TMS Portal under Settings \\> API Keys\\.
              """;

        await bot.SendMessage(
            message.Chat.Id,
            text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);
    }
}
