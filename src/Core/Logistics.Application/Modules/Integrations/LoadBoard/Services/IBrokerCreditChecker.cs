using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// The one place that decides whether a broker's credit is good enough to commit to. Shared by
/// booking and rate negotiation so both refuse the same brokers for the same reasons.
/// </summary>
public interface IBrokerCreditChecker : IApplicationService
{
    /// <summary>
    /// Fetches credit and stamps it on the listing either way (an audit trail of what was known at
    /// the time), then fails when the broker is unacceptable and the caller has no override.
    /// </summary>
    Task<Result> CheckAsync(LoadBoardListing listing, bool overrideCheck, CancellationToken ct);
}
