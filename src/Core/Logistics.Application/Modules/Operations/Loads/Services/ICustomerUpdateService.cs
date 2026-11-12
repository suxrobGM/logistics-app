using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>
/// Emails the customer about their shipment and records each attempt on <see cref="Load.CustomerUpdates"/>.
/// Requires the tenant on the unit of work. The caller owns SaveChangesAsync.
/// </summary>
public interface ICustomerUpdateService : IApplicationService
{
    /// <summary>
    /// Sends the dispatched, picked-up or delivered email for a status change. Other statuses send nothing,
    /// and an update already sent for the load is not sent again.
    /// </summary>
    Task SendStatusUpdateAsync(Load load, LoadStatus newStatus, CancellationToken ct = default);

    /// <summary>
    /// Sends the running-late email for a stop whose ETA passed its appointment window.
    /// </summary>
    Task SendLateNoticeAsync(TripStop stop, DateTime eta, CancellationToken ct = default);

    /// <summary>
    /// Sends a dispatcher-approved free-text delay notice. Ignores the customer's preferences and
    /// fails when there is nobody to send it to.
    /// </summary>
    Task<Result> SendDelayNoticeAsync(Load load, string message, CancellationToken ct = default);
}
