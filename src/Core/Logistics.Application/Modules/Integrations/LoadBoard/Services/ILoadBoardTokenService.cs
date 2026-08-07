using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// Returns a provider whose OAuth token is valid, persisting refreshed tokens on the tenant
/// configuration. Saves the scope immediately on acquisition - call before staging other changes.
/// </summary>
public interface ILoadBoardTokenService : IApplicationService
{
    Task<Result<ILoadBoardProviderService>> GetReadyProviderAsync(
        LoadBoardConfiguration configuration, CancellationToken ct = default);
}
