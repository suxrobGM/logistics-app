using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.TelegramBot.Authentication;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Commands;

internal sealed class NotifyCommand(ITenantUnitOfWork tenantUow) : ITelegramCommand
{
    public string Name => "/notify";
    public string Description => "Toggle notifications (on/off)";
    public bool RequiresAuth => true;

    public async Task ExecuteAsync(
        ITelegramBotClient bot, Message message, TelegramChatContext? context, CancellationToken ct)
    {
        var text = message.Text ?? "";
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || (parts[1] is not "on" and not "off"))
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Usage: /notify on or /notify off",
                cancellationToken: ct);
            return;
        }

        var enabled = parts[1] == "on";
        var chat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == message.Chat.Id, ct);

        if (chat is null)
        {
            await bot.SendMessage(
                message.Chat.Id,
                "Chat not found. Please reconnect with /start.",
                cancellationToken: ct);
            return;
        }

        chat.NotificationsEnabled = enabled;
        await tenantUow.SaveChangesAsync(ct);

        var status = enabled ? "enabled" : "disabled";
        await bot.SendMessage(
            message.Chat.Id,
            $"Notifications {status}.",
            cancellationToken: ct);
    }
}
