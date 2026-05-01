using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Maps a Telegram chat (private or group) to a tenant via Identity Server authentication.
/// Stored in the tenant database - each tenant manages their own connected chats.
/// </summary>
public class TelegramChat : Entity, ITenantEntity
{
    public long ChatId { get; set; }
    public TelegramChatType ChatType { get; set; }
    public TelegramChatRole? Role { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? GroupTitle { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastInteractionAt { get; set; }
}
