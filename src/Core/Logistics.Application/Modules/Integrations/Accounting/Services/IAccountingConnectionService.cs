using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// Hands out a provider client with a usable access token, refreshing it when it is about to expire.
/// When the provider refuses the refresh token the connection is flagged
/// <see cref="AccountingProviderConfiguration.NeedsReconnect"/> and saved. Expects the tenant context to be set.
/// </summary>
public interface IAccountingConnectionService : IApplicationService
{
    /// <summary>The initialized provider, or null when the connection needs the user to reconnect.</summary>
    Task<IAccountingProviderService?> GetProviderAsync(AccountingProviderConfiguration config, CancellationToken ct);

    /// <summary>
    /// The provider rejected the access token on a data call (the user may have disconnected the app on the
    /// provider's side). Forget the access token so the next run refreshes it, which tells a revoked
    /// connection from a stale token.
    /// </summary>
    Task RejectAccessTokenAsync(AccountingProviderConfiguration config, CancellationToken ct);
}
