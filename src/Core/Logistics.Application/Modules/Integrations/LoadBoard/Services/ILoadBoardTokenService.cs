using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// Returns a provider service whose OAuth token is valid, refreshing or re-acquiring and
/// persisting it on the tenant configuration when needed. Call before mutating other entities
/// in the scope - a successful token acquisition saves immediately so it survives a failed
/// vendor call afterwards.
/// </summary>
public interface ILoadBoardTokenService : IApplicationService
{
    Task<Result<ILoadBoardProviderService>> GetReadyProviderAsync(
        LoadBoardConfiguration configuration, CancellationToken ct = default);
}
