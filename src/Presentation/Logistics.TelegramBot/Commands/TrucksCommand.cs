using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Logistics.Mediator;
using Telegram.Bot;
using Telegram.Bot.Types;
using Logistics.Application.Modules.Operations.Trucks.Queries;

namespace Logistics.TelegramBot.Commands;

internal sealed class TrucksCommand(IMediator mediator) : ITelegramCommand, ICallbackHandler
{
    private const int PageSize = 5;
    private const string CallbackCommand = "trucks";

    public string Name => "/trucks";
    public string Description => "View available trucks";
    public bool RequiresAuth => true;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        await SendPageAsync(bot, message.Chat.Id, 1, null, ct);
    }

    public async Task HandleCallbackAsync(
        ITelegramBotClient bot, CallbackQuery callbackQuery, TelegramChatContext? context, CancellationToken ct)
    {
        if (callbackQuery.Message is not { } message || !callbackQuery.TryGetPage(CallbackCommand, out var page))
        {
            return;
        }

        await SendPageAsync(bot, message.Chat.Id, page, message.MessageId, ct);
    }

    private async Task SendPageAsync(
        ITelegramBotClient bot, long chatId, int page, int? editMessageId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTrucksQuery
        {
            Page = page,
            PageSize = PageSize
        }, ct);

        var trucks = result.Value?.ToList() ?? [];
        var text = TelegramMessageFormatter.FormatTrucks(trucks, page, result.TotalPages);
        var keyboard = TelegramMessageFormatter.BuildPaginationKeyboard(CallbackCommand, page, result.TotalPages);

        await bot.SendOrEditPageAsync(chatId, editMessageId, text, keyboard, ct);
    }
}
