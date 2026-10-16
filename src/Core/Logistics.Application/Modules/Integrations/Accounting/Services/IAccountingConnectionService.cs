using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// The one way handlers and jobs call the accounting provider. Loads the active QuickBooks connection and
/// hands the call a provider client with a usable access token, refreshing it when it is about to expire.
/// When the provider refuses the refresh token the connection is flagged
/// <see cref="AccountingProviderConfiguration.NeedsReconnect"/> and saved; when it rejects the access token
/// on a data call (the user may have disconnected the app on the provider's side), the access token is
/// forgotten so the next run refreshes it, which tells a revoked connection from a stale token.
/// Every such case is a failed <see cref="Result"/> with a user-facing message. Expects the tenant context to be set.
/// </summary>
public interface IAccountingConnectionService : IApplicationService
{
    Task<Result<T>> ExecuteAsync<T>(
        Func<IAccountingProviderService, AccountingProviderConfiguration, Task<T>> call, CancellationToken ct);

    /// <summary>The call returns its own <see cref="Result"/>, so it can fail on its own checks.</summary>
    Task<Result> ExecuteAsync(
        Func<IAccountingProviderService, AccountingProviderConfiguration, Task<Result>> call, CancellationToken ct);
}
