using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logistics.TelegramBot.Commands;

/// <summary>Paging for the list commands (/loads, /trucks, /trips).</summary>
internal static class PagedListExtensions
{
    extension(ITelegramBotClient bot)
    {
        /// <summary>
        /// Rewrites the list message in place when a page button was pressed, so paging does not
        /// flood the chat; otherwise sends a new message.
        /// </summary>
        public async Task SendOrEditPageAsync(
            long chatId,
            int? editMessageId,
            string text,
            InlineKeyboardMarkup? keyboard,
            CancellationToken ct)
        {
            if (editMessageId is not null)
            {
                await bot.EditMessageText(
                    chatId, editMessageId.Value, text,
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: keyboard,
                    cancellationToken: ct);
                return;
            }

            await bot.SendMessage(
                chatId, text,
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
