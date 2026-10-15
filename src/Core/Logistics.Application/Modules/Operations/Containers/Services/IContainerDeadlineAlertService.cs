using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Containers.Services;

/// <summary>
/// Notifies the current tenant's dispatchers about containers whose last free day or empty-return
/// deadline is today, tomorrow, or already passed.
/// </summary>
public interface IContainerDeadlineAlertService : IApplicationService
{
    /// <summary>Requires the tenant to be set on the unit of work. Returns the alerts sent.</summary>
    Task<int> SendAlertsAsync(DateTime now, CancellationToken ct = default);
}
