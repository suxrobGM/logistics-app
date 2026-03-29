using Logistics.TelegramBot.Authentication;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Commands;

internal interface ITelegramCommand
{
    string Name { get; }
    string Description { get; }
    bool RequiresAuth { get; }
    Task ExecuteAsync(ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct);
}

internal interface ICallbackHandler
{
    Task HandleCallbackAsync(
        ITelegramBotClient bot,
        CallbackQuery callbackQuery,
        TelegramChatContext? context,
        CancellationToken ct);
}
