using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

public static class TelegramRecipients
{
    /// <summary>The Telegram audience a tenant role belongs to; null for roles the bot never notifies.</summary>
    public static TelegramChatRole? RoleOf(string? tenantRoleName) => tenantRoleName switch
    {
        TenantRoles.Driver => TelegramChatRole.Driver,
        TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher => TelegramChatRole.Dispatcher,
        _ => null
    };

    /// <summary>
    /// The connected chat the bot may run commands for: its row still exists and its connecting
    /// employee is still on the tenant and Active. Null otherwise.
    /// </summary>
    public static Task<TelegramChat?> FindAuthorizedChatAsync(
        ITenantUnitOfWork tenantUow, long chatId, CancellationToken ct)
    {
        var activeEmployees = tenantUow.Repository<Employee>().Query().Where(e => e.Status == EmployeeStatus.Active);
        return tenantUow.Repository<TelegramChat>().Query()
            .FirstOrDefaultAsync(c => c.ChatId == chatId && activeEmployees.Any(e => e.Id == c.UserId), ct);
    }

    /// <summary>
    /// The chats a tenant notification goes to. Only chats whose connecting employee is still on the
    /// tenant and Active qualify; group chats get every notification, private chats only those for
    /// the employee's current role.
    /// </summary>
    public static async Task<List<TelegramChat>> ListAsync(
        ITenantUnitOfWork tenantUow, TelegramChatRole? targetRole, CancellationToken ct)
    {
        var chats = await tenantUow.Repository<TelegramChat>().Query()
            .Where(c => c.NotificationsEnabled)
            .Join(tenantUow.Repository<Employee>().Query().Where(e => e.Status == EmployeeStatus.Active),
                c => c.UserId, e => (Guid?)e.Id,
                (c, e) => new { Chat = c, RoleName = e.Role != null ? e.Role.Name : null })
            .ToListAsync(ct);

        return chats
            .Where(x => x.Chat.ChatType == TelegramChatType.Group || targetRole is null || RoleOf(x.RoleName) == targetRole)
            .Select(x => x.Chat)
            .ToList();
    }
}