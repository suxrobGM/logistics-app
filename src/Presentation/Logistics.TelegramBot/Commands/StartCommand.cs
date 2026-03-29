using Logistics.Domain.Primitives.Enums;
using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Logistics.TelegramBot.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logistics.TelegramBot.Commands;

internal sealed class StartCommand(
    TelegramAuthService authService,
    IOptions<TelegramBotOptions> options) : ITelegramCommand
{
    public string Name => "/start";
    public string Description => "Welcome message and login";
    public bool RequiresAuth => false;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        if (context is not null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                $"Welcome back\\! You are connected to *{TelegramMessageFormatter.Escape(context.TenantName)}*\\.\n\nUse /help to see available commands\\.",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
            return;
        }

        // Check if this is a deep link callback (bot polls for consumed state)
        var text = message.Text ?? "";
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            var state = parts[1];
            var result = await authService.CheckLoginStateAsync(state, ct);
            if (result is not null)
            {
                await bot.SendMessage(
                    message.Chat.Id,
                    $"Connected to *{TelegramMessageFormatter.Escape(result.TenantName)}*\\! Use /help to see available commands\\.",
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct);
                return;
            }
        }

        // Show login button
        var chatType = message.Chat.Type is ChatType.Group or ChatType.Supergroup
            ? TelegramChatType.Group
            : TelegramChatType.Private;

        var loginState = await authService.CreateLoginStateAsync(message.Chat.Id, chatType, ct);
        var identityUrl = options.Value.IdentityServerUrl?.TrimEnd('/');
        var loginUrl = $"{identityUrl}/telegram/login?state={loginState}";

        var keyboard = new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithUrl("Login with LogisticsX", loginUrl)]
        ]);

        var welcomeText = chatType == TelegramChatType.Group
            ? "Welcome to *LogisticsX Bot*\\!\n\nAn owner, manager, or dispatcher must log in to connect this group\\."
            : "Welcome to *LogisticsX Bot*\\!\n\nLog in with your LogisticsX account to get started\\.";

        await bot.SendMessage(
            message.Chat.Id,
            welcomeText,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
}
