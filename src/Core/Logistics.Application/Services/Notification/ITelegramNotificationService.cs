using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Services;

public interface ITelegramNotificationService
{
    /// <summary>
    /// Sends a Telegram notification to all users with the specified role in the tenant.
    /// </summary>
    Task SendNotificationAsync(
        Guid tenantId,
        string title,
        string message,
        TelegramChatRole? targetRole = null,
        CancellationToken ct = default);
}
