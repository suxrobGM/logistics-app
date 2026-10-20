namespace Logistics.Domain.Options;

/// <summary>
///     Where website demo requests, contact messages and received email that no feature handles are sent.
/// </summary>
public sealed class OwnerNotificationOptions
{
    public const string SectionName = "OwnerNotifications";

    public string? Email { get; set; }
}
