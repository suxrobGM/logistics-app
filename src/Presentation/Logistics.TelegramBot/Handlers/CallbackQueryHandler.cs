using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Handlers;

/// <summary>
/// Handles Telegram callback queries for interactive elements like inline keyboards.
/// Parses the callback data to determine which command and action to invoke,
/// then dispatches to the appropriate command handler.
/// </summary>
internal sealed class CallbackQueryHandler(IEnumerable<ITelegramCommand> telegramCommands)
{
    private readonly Dictionary<string, ITelegramCommand> commands = telegramCommands.ToDictionary(c => c.Name);

    /// <summary>
    /// Processes an incoming callback query from Telegram.
    /// </summary>
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

        // Parse callback data: "command:action:params" (e.g., "loads:page:2")
        var parts = data.Split(':');
        if (parts.Length < 2)
            return;

        var commandName = $"/{parts[0]}";
        if (!commands.TryGetValue(commandName, out var command))
            return;

        if (command.RequiresAuth && context is null)
            return;

        // Create a synthetic message from the callback for the command to use
        if (callbackQuery.Message is not Message message)
            return;

        // Store callback data in message text so commands can parse pagination params

        if (command is ICallbackHandler callbackHandler)
        {
            await callbackHandler.HandleCallbackAsync(bot, callbackQuery, context, ct);
        }
    }
}
