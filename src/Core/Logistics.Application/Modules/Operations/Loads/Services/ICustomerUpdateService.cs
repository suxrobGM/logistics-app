using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>Records each attempt on <see cref="Load.CustomerUpdates"/>. The caller owns SaveChangesAsync.</summary>
public interface ICustomerUpdateService : IApplicationService
{
    /// <summary>Other statuses send nothing, and an update already sent is not sent again.</summary>
    Task SendStatusUpdateAsync(Load load, LoadStatus newStatus, CancellationToken ct = default);

    Task SendLateNoticeAsync(TripStop stop, DateTime eta, CancellationToken ct = default);

    /// <summary>Ignores the customer's preferences, since a dispatcher approved it, and fails with no recipient.</summary>
    Task<Result> SendDelayNoticeAsync(Load load, string message, CancellationToken ct = default);
}
