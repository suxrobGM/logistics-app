using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Logistics.TelegramBot.Services;

internal sealed class TelegramNotificationService(
    ITelegramBotClient bot,
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramNotificationService> logger) : ITelegramNotificationService
{
    public async Task SendNotificationAsync(
        Guid tenantId,
        string title,
        string message,
        TelegramChatRole? targetRole = null,
        CancellationToken ct = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

            if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.TelegramBot))
                return;

            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            await tenantUow.SetCurrentTenantByIdAsync(tenantId);

            var chats = await tenantUow.Repository<TelegramChat>()
                .GetListAsync(c => c.NotificationsEnabled, ct: ct);

            var formattedMessage = $"*{Escape(title)}*\n{Escape(message)}";

            foreach (var chat in chats)
            {
                // Group chats get all notifications; private chats filter by role
                if (chat.ChatType == TelegramChatType.Private &&
                    targetRole is not null &&
                    chat.Role != targetRole)
                {
                    continue;
                }

                try
                {
                    await bot.SendMessage(
                        chat.ChatId,
                        formattedMessage,
                        parseMode: ParseMode.MarkdownV2,
                        cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Failed to send Telegram notification to chat {ChatId}", chat.ChatId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to send Telegram notifications for tenant {TenantId}", tenantId);
        }
    }

    private static string Escape(string text)
    {
        return Formatters.TelegramMessageFormatter.Escape(text);
    }
}
