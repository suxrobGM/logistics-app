using Logistics.Application.Abstractions.Common;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// The one way handlers and jobs call a load board. Makes sure the provider holds a valid token,
/// re-authenticates once when the vendor answers 401, and turns a vendor failure into a failed
/// <see cref="Result"/> carrying the vendor's message. Persists renewed tokens on the tenant
/// configuration by saving the scope, so call it before staging other changes.
/// </summary>
public interface ILoadBoardProviderGateway : IApplicationService
{
    Task<Result<T>> ExecuteAsync<T>(
        LoadBoardConfiguration configuration,
        Func<ILoadBoardProviderService, Task<T>> call,
        CancellationToken ct = default);

    Task<Result> ExecuteAsync(
        LoadBoardConfiguration configuration,
        Func<ILoadBoardProviderService, Task> call,
        CancellationToken ct = default);

    /// <summary>
    /// Proves the stored credentials work: acquires a fresh token (token providers) or validates
    /// the API key, whatever token is already cached.
    /// </summary>
    Task<Result> TestConnectionAsync(LoadBoardConfiguration configuration, CancellationToken ct = default);
}
