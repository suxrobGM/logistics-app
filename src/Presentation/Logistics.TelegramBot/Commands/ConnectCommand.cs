using Logistics.Domain.Primitives.Enums;
using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Formatters;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramChatType = Logistics.Domain.Primitives.Enums.TelegramChatType;

namespace Logistics.TelegramBot.Commands;

internal sealed class ConnectCommand(TelegramAuthService authService) : ITelegramCommand, ICallbackHandler
{
    public string Name => "/connect";
    public string Description => "Connect to your company using an API key";
    public bool RequiresAuth => false;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        // Delete the message containing the API key for security
        try
        {
            await bot.DeleteMessage(message.Chat.Id, message.MessageId, cancellationToken: ct);
        }
        catch
        {
            // May fail in groups if bot lacks delete permissions
        }

        var text = message.Text ?? "";
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Usage: `/connect your_api_key_here`",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
            return;
        }

        var rawApiKey = parts[1].Trim();
        var chatType = message.Chat.Type is ChatType.Group or ChatType.Supergroup
            ? TelegramChatType.Group
            : TelegramChatType.Private;

        // For groups, connect without role selection
        if (chatType == TelegramChatType.Group)
        {
            var result = await authService.AuthenticateAsync(
                message.Chat.Id,
                rawApiKey,
                chatType,
                role: null,
                username: message.From?.Username,
                firstName: message.From?.FirstName,
                groupTitle: message.Chat.Title,
                ct);

            var responseText = result.IsSuccess
                ? $"Connected to *{TelegramMessageFormatter.Escape(result.Context!.TenantName)}*\\! Use /help to see available commands\\."
                : $"Connection failed: {TelegramMessageFormatter.Escape(result.Error!)}";

            await bot.SendMessage(
                message.Chat.Id,
                responseText,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
            return;
        }

        // For private chats, authenticate first then ask for role
        var authResult = await authService.AuthenticateAsync(
            message.Chat.Id,
            rawApiKey,
            chatType,
            role: null,
            username: message.From?.Username,
            firstName: message.From?.FirstName,
            groupTitle: null,
            ct);

        if (!authResult.IsSuccess)
        {
            await bot.SendMessage(
                message.Chat.Id,
                $"Connection failed: {TelegramMessageFormatter.Escape(authResult.Error!)}",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Dispatcher", "connect:role:dispatcher"),
                InlineKeyboardButton.WithCallbackData("Driver", "connect:role:driver")
            ]
        ]);

        await bot.SendMessage(
            message.Chat.Id,
            $"Connected to *{TelegramMessageFormatter.Escape(authResult.Context!.TenantName)}*\\!\n\nSelect your role:",
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    public async Task HandleCallbackAsync(
        ITelegramBotClient bot, CallbackQuery callbackQuery, TelegramChatContext? context, CancellationToken ct)
    {
        var data = callbackQuery.Data;
        if (data is null || !data.StartsWith("connect:role:"))
            return;

        var roleName = data["connect:role:".Length..];
        var role = roleName == "driver" ? TelegramChatRole.Driver : TelegramChatRole.Dispatcher;

        var chatId = callbackQuery.Message?.Chat.Id;
        if (chatId is null)
            return;

        await authService.UpdateRoleAsync(chatId.Value, role, ct);

        // Edit the original message to confirm
        if (callbackQuery.Message is not null)
        {
            await bot.EditMessageText(
                chatId.Value,
                callbackQuery.Message.MessageId,
                $"Connected as *{TelegramMessageFormatter.Escape(role.GetDescription())}*\\! Use /help to see available commands\\.",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
        }
    }
}
