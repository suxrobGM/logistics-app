using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

public interface ITelegramRecipients : IApplicationService
{
    /// <summary>
    /// The connected chat the bot may run commands for: its row still exists and its connecting
    /// employee is still on the tenant and Active. Null otherwise.
    /// </summary>
    Task<TelegramChat?> FindAuthorizedChatAsync(long chatId, CancellationToken ct);

    /// <summary>
    /// The chats a tenant notification goes to. Only chats whose connecting employee is still on the
    /// tenant and Active qualify; group chats get every notification, private chats only those for
    /// the employee's current role.
    /// </summary>
    Task<List<TelegramChat>> ListAsync(TelegramChatRole? targetRole, CancellationToken ct);
}
