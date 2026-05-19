namespace Logistics.Application.Modules.Compliance.Safety.Services;

/// <summary>
/// Generates notifications for driver licenses approaching expiry. Invoked daily by Hangfire.
/// </summary>
public interface ILicenseExpiryReminderService : IApplicationService
{
    Task ProcessLicenseExpiryRemindersAsync(CancellationToken ct = default);
}
