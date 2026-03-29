using Logistics.TelegramBot.Authentication;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Commands;

internal sealed class DisconnectCommand(TelegramAuthService authService) : ITelegramCommand
{
    public string Name => "/disconnect";
    public string Description => "Disconnect from your company";
    public bool RequiresAuth => true;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        var success = await authService.DisconnectAsync(message.Chat.Id, ct);

        var text = success
            ? "Disconnected successfully. Use /start to reconnect."
            : "You are not connected.";

        await bot.SendMessage(message.Chat.Id, text, cancellationToken: ct);
    }
}
