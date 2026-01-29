namespace Logistics.Application.Services;

public interface ICertificationReminderService
{
    /// <summary>
    /// Check for expiring certifications and send reminder notifications
    /// </summary>
    Task ProcessExpiringCertificationsAsync(CancellationToken ct = default);
}
