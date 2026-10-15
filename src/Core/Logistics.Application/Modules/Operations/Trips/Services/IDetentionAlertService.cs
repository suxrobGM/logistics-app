namespace Logistics.Application.Modules.Operations.Trips.Services;

/// <summary>
/// Tells the current tenant's dispatchers when a truck has stayed at a stop past its free time.
/// </summary>
public interface IDetentionAlertService : IApplicationService
{
    /// <summary>
    /// Requires the tenant to be set on the unit of work. Alerts each overstaying stop once and
    /// saves the alert time. Returns the alerts sent.
    /// </summary>
    Task<int> SendAlertsAsync(DateTime now, CancellationToken ct = default);
}
