using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Temporary state for the Telegram bot login flow.
/// Created when a user clicks "Login" in the bot, consumed when they complete authentication.
/// Stored in master DB so it's accessible before tenant resolution.
/// </summary>
public class TelegramLoginState : Entity, IMasterEntity
{
    public required string State { get; set; }
    public long ChatId { get; set; }
    public TelegramChatType ChatType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsConsumed { get; set; }

    /// <summary>
    /// Set after the user authenticates — the bot polls for this.
    /// </summary>
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? TenantName { get; set; }
    public TelegramChatRole? ResolvedRole { get; set; }
}
