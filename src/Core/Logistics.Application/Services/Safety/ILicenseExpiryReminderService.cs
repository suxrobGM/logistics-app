namespace Logistics.Application.Services;

/// <summary>
/// Generates notifications for driver licenses approaching expiry. Invoked daily by Hangfire.
/// </summary>
public interface ILicenseExpiryReminderService
{
    Task ProcessLicenseExpiryRemindersAsync(CancellationToken ct = default);
}
