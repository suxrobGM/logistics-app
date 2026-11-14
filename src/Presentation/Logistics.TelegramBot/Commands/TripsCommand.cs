using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Logistics.Mediator;
using Telegram.Bot;
using Telegram.Bot.Types;
using Logistics.Application.Modules.Operations.Trips.Queries;

namespace Logistics.TelegramBot.Commands;

internal sealed class TripsCommand(IMediator mediator) : ITelegramCommand, ICallbackHandler
{
    private const int PageSize = 5;

    public string Name => "/trips";
    public string Description => "View active trips";
    public bool RequiresAuth => true;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        await SendPageAsync(bot, message.Chat.Id, 1, null, ct);
    }

    public async Task HandleCallbackAsync(
        ITelegramBotClient bot, long chatId, int messageId, int page, CancellationToken ct)
    {
        await SendPageAsync(bot, chatId, page, messageId, ct);
    }

    private async Task SendPageAsync(
        ITelegramBotClient bot, long chatId, int page, int? editMessageId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTripsQuery
        {
            ActiveOnly = true,
            Page = page,
            PageSize = PageSize,
            OrderBy = "-createdAt"
        }, ct);

        var trips = result.Value?.ToList() ?? [];
        var text = TelegramMessageFormatter.FormatTrips(trips, page, result.TotalPages);
        var keyboard = TelegramMessageFormatter.BuildPaginationKeyboard(Name[1..], page, result.TotalPages);

        await bot.SendOrEditPageAsync(chatId, editMessageId, text, keyboard, ct);
    }
}
