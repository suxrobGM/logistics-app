namespace Logistics.Application.Services;

public interface IMaintenanceReminderService
{
    /// <summary>
    /// Check for upcoming and overdue maintenance schedules and send reminders
    /// </summary>
    Task ProcessMaintenanceRemindersAsync(CancellationToken ct = default);
}
