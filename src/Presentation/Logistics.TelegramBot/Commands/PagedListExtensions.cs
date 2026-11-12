using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logistics.TelegramBot.Commands;

/// <summary>Paging for the list commands (/loads, /trucks, /trips).</summary>
internal static class PagedListExtensions
{
    extension(CallbackQuery callbackQuery)
    {
        /// <summary>
        /// Reads the page number from a Prev/Next button whose data is "{command}:page:{n}", as built by
        /// <c>TelegramMessageFormatter.BuildPaginationKeyboard</c>.
        /// </summary>
        public bool TryGetPage(string command, out int page)
        {
            page = 0;
            var prefix = $"{command}:page:";
            var data = callbackQuery.Data;
            if (data is null || !data.StartsWith(prefix))
            {
                return false;
            }

            return int.TryParse(data[prefix.Length..], out page);
        }
    }

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
