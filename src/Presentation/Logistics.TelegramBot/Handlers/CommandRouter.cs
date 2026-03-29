using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Handlers;

/// <summary>
/// Routes incoming Telegram messages to the appropriate command handlers based on the command name.
/// </summary>
internal sealed class CommandRouter(IEnumerable<ITelegramCommand> telegramCommands)
{
    private readonly Dictionary<string, ITelegramCommand> commands = telegramCommands.ToDictionary(c => c.Name);

    public async Task RouteAsync(
        ITelegramBotClient bot,
        Message message,
        TelegramChatContext? context,
        CancellationToken ct)
    {
        var text = message.Text;
        if (string.IsNullOrEmpty(text) || !text.StartsWith('/'))
            return;

        // Parse command name (handles "/command@botname" format in groups)
        var spaceIndex = text.IndexOf(' ');
        var commandPart = spaceIndex > 0 ? text[..spaceIndex] : text;
        var atIndex = commandPart.IndexOf('@');
        var commandName = atIndex > 0 ? commandPart[..atIndex] : commandPart;

        if (!commands.TryGetValue(commandName, out var command))
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Unknown command. Use /help to see available commands.",
                cancellationToken: ct);
            return;
        }

        if (command.RequiresAuth && context is null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "You need to connect first. Use /start to authenticate.",
                cancellationToken: ct);
            return;
        }

        await command.ExecuteAsync(bot, message, context, ct);
    }
}
