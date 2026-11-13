using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Maps a Telegram chat (private or group) to a tenant via Identity Server authentication.
/// Stored in the tenant database - each tenant manages their own connected chats. The connecting
/// employee's role and status are read from <see cref="Employee"/> when sending, never copied here.
/// </summary>
public class TelegramChat : Entity, ITenantEntity
{
    public long ChatId { get; set; }
    public TelegramChatType ChatType { get; set; }

    /// <summary>The employee who connected the chat.</summary>
    public Guid? UserId { get; set; }

    public bool NotificationsEnabled { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}
