using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>
/// Broadcasts filing-deadline reminders (Apr 30 / Jul 31 / Oct 31 / Jan 31) to tenants with
/// the IFTA feature, at 30/14/7/3/1 days before the deadline.
/// </summary>
public interface IIftaFilingReminderService : IApplicationService
{
    Task ProcessRemindersAsync(CancellationToken ct = default);
}
