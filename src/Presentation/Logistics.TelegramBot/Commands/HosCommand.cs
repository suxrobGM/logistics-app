using Logistics.Application.Queries;
using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Commands;

internal sealed class HosCommand(IMediator mediator) : ITelegramCommand
{
    public string Name => "/hos";
    public string Description => "View driver HOS status";
    public bool RequiresAuth => true;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllDriversHosStatusQuery(), ct);

        if (!result.IsSuccess || result.Value is null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Failed to retrieve HOS status.",
                cancellationToken: ct);
            return;
        }

        var text = TelegramMessageFormatter.FormatHosStatus(result.Value);

        await bot.SendMessage(
            message.Chat.Id,
            text,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);
    }
}
