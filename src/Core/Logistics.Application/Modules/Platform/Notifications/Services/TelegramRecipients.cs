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
    public static async Task<TelegramChat?> FindAuthorizedChatAsync(
        ITenantUnitOfWork tenantUow, long chatId, CancellationToken ct)
    {
        var chat = await tenantUow.Repository<TelegramChat>().Query()
            .FirstOrDefaultAsync(c => c.ChatId == chatId && c.UserId != null, ct);

        if (chat is null)
            return null;

        var employeeActive = await tenantUow.Repository<Employee>().Query()
            .AnyAsync(e => e.Id == chat.UserId && e.Status == EmployeeStatus.Active, ct);

        return employeeActive ? chat : null;
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
            .Where(c => c.NotificationsEnabled && c.UserId != null)
            .ToListAsync(ct);

        var userIds = chats.Select(c => c.UserId!.Value).Distinct().ToList();
        var roleNames = await tenantUow.Repository<Employee>().Query()
            .Where(e => userIds.Contains(e.Id) && e.Status == EmployeeStatus.Active)
            .Select(e => new { e.Id, RoleName = e.Role != null ? e.Role.Name : null })
            .ToDictionaryAsync(e => e.Id, e => e.RoleName, ct);

        return chats
            .Where(c => roleNames.TryGetValue(c.UserId!.Value, out var roleName)
                && (c.ChatType == TelegramChatType.Group || targetRole is null || RoleOf(roleName) == targetRole))
            .ToList();
    }
}
