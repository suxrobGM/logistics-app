using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

public sealed class TelegramRecipients(ITenantUnitOfWork tenantUow) : ITelegramRecipients
{
    /// <summary>The Telegram audience a tenant role belongs to; null for roles the bot never notifies.</summary>
    public static TelegramChatRole? GetChatRole(string? tenantRoleName) => tenantRoleName switch
    {
        TenantRoles.Driver => TelegramChatRole.Driver,
        TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher => TelegramChatRole.Dispatcher,
        _ => null
    };

    public Task<TelegramChat?> FindAuthorizedChatAsync(long chatId, CancellationToken ct)
    {
        var activeEmployees = tenantUow.Repository<Employee>().Query().Where(e => e.Status == EmployeeStatus.Active);
        return tenantUow.Repository<TelegramChat>().Query()
            .FirstOrDefaultAsync(c => c.ChatId == chatId && activeEmployees.Any(e => e.Id == c.UserId), ct);
    }

    public async Task<List<TelegramChat>> ListAsync(TelegramChatRole? targetRole, CancellationToken ct)
    {
        var chats = await tenantUow.Repository<TelegramChat>().Query()
            .Where(c => c.NotificationsEnabled)
            .Join(tenantUow.Repository<Employee>().Query().Where(e => e.Status == EmployeeStatus.Active),
                c => c.UserId, e => (Guid?)e.Id,
                (c, e) => new { Chat = c, RoleName = e.Role != null ? e.Role.Name : null })
            .ToListAsync(ct);

        return chats
            .Where(x => x.Chat.ChatType == TelegramChatType.Group || targetRole is null || GetChatRole(x.RoleName) == targetRole)
            .Select(x => x.Chat)
            .ToList();
    }
}