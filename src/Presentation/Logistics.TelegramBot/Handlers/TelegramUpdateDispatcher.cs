using System.Collections.Concurrent;
using Logistics.Domain.Persistence;
using Logistics.TelegramBot.Authentication;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Handlers;

internal sealed class TelegramUpdateDispatcher(
    ITelegramBotClient bot,
    TelegramAuthService authService,
    ITenantUnitOfWork tenantUow,
    CommandRouter commandRouter,
    CallbackQueryHandler callbackQueryHandler,
    ILogger<TelegramUpdateDispatcher> logger)
{
    private static readonly ConcurrentDictionary<long, DateTime> LastCommandTime = new();
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
    private const int MaxCommandsPerMinute = 20;
    private static readonly ConcurrentDictionary<long, int> CommandCounts = new();

    public async Task DispatchAsync(Update update, CancellationToken ct)
    {
        try
        {
            var chatId = ExtractChatId(update);
            if (chatId is null)
                return;

            // Input length validation
            if (update.Message?.Text?.Length > 500)
            {
                await bot.SendMessage(chatId.Value, "Message too long. Maximum 500 characters.", cancellationToken: ct);
                return;
            }

            // Rate limiting per chat
            if (IsRateLimited(chatId.Value))
            {
                await bot.SendMessage(chatId.Value, "Too many requests. Please slow down.", cancellationToken: ct);
                return;
            }

            var context = TelegramAuthService.ResolveChatContext(chatId.Value);

            // Set tenant context if authenticated
            if (context is not null)
            {
                await tenantUow.SetCurrentTenantByIdAsync(context.TenantId);
            }

            switch (update.Type)
            {
                case UpdateType.Message when update.Message?.Text is not null:
                    if (update.Message.Text.StartsWith('/'))
                    {
                        await commandRouter.RouteAsync(bot, update.Message, context, ct);
                    }
                    else if (context is not null)
                    {
                        await bot.SendMessage(
                            chatId.Value,
                            "Use /help to see available commands.",
                            cancellationToken: ct);
                    }
                    break;

                case UpdateType.CallbackQuery when update.CallbackQuery is not null:
                    await callbackQueryHandler.HandleAsync(bot, update.CallbackQuery, context, ct);
                    break;

                // Handle bot blocked by user or kicked from group
                case UpdateType.MyChatMember when update.MyChatMember is not null:
                    await HandleBotMembershipChangeAsync(update.MyChatMember, ct);
                    break;
            }

            // Fire-and-forget LastInteractionAt update
            if (context is not null)
            {
                _ = Task.Run(() => UpdateLastInteractionAsync(chatId.Value, context), CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error dispatching Telegram update {UpdateId}", update.Id);
        }
    }

    private static long? ExtractChatId(Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => update.Message?.Chat.Id,
            UpdateType.CallbackQuery => update.CallbackQuery?.Message?.Chat.Id,
            UpdateType.MyChatMember => update.MyChatMember?.Chat.Id,
            _ => null
        };
    }

    /// <summary>
    /// Implements simple rate limiting by tracking the last command time and count of commands per chat ID.
    /// If a chat exceeds the maximum allowed commands within the rate limit window, it will be temporarily blocked from sending more commands until the window resets.
    /// This helps protect against spam and abuse while allowing legitimate
    /// </summary>
    private static bool IsRateLimited(long chatId)
    {
        var now = DateTime.UtcNow;

        if (LastCommandTime.TryGetValue(chatId, out var lastTime) &&
            now - lastTime < RateLimitWindow)
        {
            var count = CommandCounts.AddOrUpdate(chatId, 1, (_, c) => c + 1);
            return count > MaxCommandsPerMinute;
        }

        LastCommandTime[chatId] = now;
        CommandCounts[chatId] = 1;
        return false;
    }

    private async Task HandleBotMembershipChangeAsync(
        ChatMemberUpdated memberUpdate,
        CancellationToken ct)
    {
        var newStatus = memberUpdate.NewChatMember.Status;

        // Bot was blocked by user or kicked from group
        if (newStatus is ChatMemberStatus.Kicked or ChatMemberStatus.Left)
        {
            var chatId = memberUpdate.Chat.Id;
            logger.LogInformation(
                "Bot removed from chat {ChatId} (status: {Status}), cleaning up",
                chatId, newStatus);

            await authService.DisconnectAsync(chatId, ct);
        }
    }

    private async Task UpdateLastInteractionAsync(long chatId, TelegramChatContext context)
    {
        try
        {
            var chat = await tenantUow.Repository<Domain.Entities.TelegramChat>()
                .GetAsync(c => c.ChatId == chatId);

            if (chat is not null)
            {
                chat.LastInteractionAt = DateTime.UtcNow;
                await tenantUow.SaveChangesAsync();
            }
        }
        catch
        {
            // Best-effort — don't fail request for tracking
        }
    }
}
