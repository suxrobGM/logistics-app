namespace Logistics.Domain.Options;

/// <summary>
///     Bound from the flat <c>NotificationEmail</c> setting: where website demo requests and contact
///     messages are emailed.
/// </summary>
public sealed class LeadNotificationOptions
{
    public string? Email { get; set; }
}
