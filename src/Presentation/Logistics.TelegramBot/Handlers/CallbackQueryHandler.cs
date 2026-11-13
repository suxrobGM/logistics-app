using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Handlers;

/// <summary>Routes inline-keyboard page buttons to the list command that built them.</summary>
internal sealed class CallbackQueryHandler(IEnumerable<ITelegramCommand> telegramCommands)
{
    private readonly Dictionary<string, ITelegramCommand> _commands = telegramCommands.ToDictionary(c => c.Name);

    public async Task HandleAsync(
        ITelegramBotClient bot,
        CallbackQuery callbackQuery,
        TelegramChatContext? context,
        CancellationToken ct)
    {
        var data = callbackQuery.Data;
        if (string.IsNullOrEmpty(data))
            return;

        // Acknowledge the callback to dismiss loading indicator
        await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

        // Callback data is "{command}:page:{n}", as built by TelegramMessageFormatter.BuildPaginationKeyboard.
        var parts = data.Split(':');
        if (parts.Length != 3 || parts[1] != "page" || !int.TryParse(parts[2], out var page))
            return;

        if (!_commands.TryGetValue($"/{parts[0]}", out var command))
            return;

        if (command.RequiresAuth && context is null)
            return;

        if (callbackQuery.Message is not Message message)
            return;

        if (command is ICallbackHandler callbackHandler)
        {
            await callbackHandler.HandleCallbackAsync(bot, message.Chat.Id, message.MessageId, page, ct);
        }
    }
}
