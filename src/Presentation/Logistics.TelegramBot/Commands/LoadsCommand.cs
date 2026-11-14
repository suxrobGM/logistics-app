using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Logistics.Mediator;
using Telegram.Bot;
using Telegram.Bot.Types;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Persistence;

namespace Logistics.TelegramBot.Commands;

internal sealed class LoadsCommand(IMediator mediator, ITenantUnitOfWork tenantUow) : ITelegramCommand, ICallbackHandler
{
    private const int PageSize = 5;

    public string Name => "/loads";
    public string Description => "View active loads";
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
        var result = await mediator.Send(new GetLoadsQuery
        {
            ActiveOnly = true,
            Page = page,
            PageSize = PageSize,
            OrderBy = "-CreatedAt"
        }, ct);

        var loads = result.Value?.ToList() ?? [];
        var text = TelegramMessageFormatter.FormatLoads(
            loads, tenantUow.GetCurrentTenant().Currency, page, result.TotalPages);
        var keyboard = TelegramMessageFormatter.BuildPaginationKeyboard(Name[1..], page, result.TotalPages);

        await bot.SendOrEditPageAsync(chatId, editMessageId, text, keyboard, ct);
    }
}
