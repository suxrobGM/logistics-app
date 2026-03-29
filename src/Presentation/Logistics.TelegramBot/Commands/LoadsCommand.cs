using Logistics.Application.Queries;
using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Commands;

internal sealed class LoadsCommand(IMediator mediator) : ITelegramCommand, ICallbackHandler
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
        ITelegramBotClient bot, CallbackQuery callbackQuery, TelegramChatContext? context, CancellationToken ct)
    {
        var data = callbackQuery.Data;
        if (data is null || !data.StartsWith("loads:page:"))
            return;

        if (!int.TryParse(data["loads:page:".Length..], out var page))
            return;

        var chatId = callbackQuery.Message?.Chat.Id;
        var messageId = callbackQuery.Message?.MessageId;
        if (chatId is null)
            return;

        await SendPageAsync(bot, chatId.Value, page, messageId, ct);
    }

    private async Task SendPageAsync(
        ITelegramBotClient bot, long chatId, int page, int? editMessageId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoadsQuery
        {
            OnlyActiveLoads = true,
            Page = page,
            PageSize = PageSize,
            OrderBy = "-CreatedAt"
        }, ct);

        var loads = result.Value?.ToList() ?? [];
        var text = TelegramMessageFormatter.FormatLoads(loads, page, result.TotalPages);
        var keyboard = TelegramMessageFormatter.BuildPaginationKeyboard("loads", page, result.TotalPages);

        if (editMessageId is not null)
        {
            await bot.EditMessageText(
                chatId, editMessageId.Value, text,
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
        else
        {
            await bot.SendMessage(
                chatId, text,
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
