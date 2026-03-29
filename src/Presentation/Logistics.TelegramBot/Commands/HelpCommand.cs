using Logistics.TelegramBot.Authentication;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Commands;

internal sealed class HelpCommand : ITelegramCommand
{
    public string Name => "/help";
    public string Description => "Show available commands";
    public bool RequiresAuth => false;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        const string text = """
            *Available Commands*

            /start \- Connect to your company
            /disconnect \- Disconnect from your company
            /loads \- View active loads
            /trucks \- View available trucks
            /trips \- View active trips
            /hos \- View driver HOS status
            /notify `on\|off` \- Toggle notifications
            /help \- Show this help message
            """;

        await bot.SendMessage(
            message.Chat.Id,
            text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);
    }
}
