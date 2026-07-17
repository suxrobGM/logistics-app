using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// Looks up broker credit standing by MC number before booking load-board loads.
/// Results are cached per tenant (24h TTL) and double as an audit trail of what
/// was known at booking time.
/// </summary>
public interface IBrokerCreditService
{
    /// <summary>
    /// Returns the broker's credit standing, or null when the MC number is missing/invalid
    /// and no data source could provide anything.
    /// </summary>
    Task<BrokerCreditDto?> GetBrokerCreditAsync(string? mcNumber, CancellationToken ct = default);
}
