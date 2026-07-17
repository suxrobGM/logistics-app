using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Services;

/// <summary>
/// Pulls fuel card transactions from the current tenant's active providers, stages them
/// idempotently, auto-matches trucks, and materializes expenses. Shared by the manual
/// sync command and the nightly Hangfire job.
/// </summary>
public interface IFuelCardSyncService : IApplicationService
{
    Task<FuelCardSyncResultDto> SyncCurrentTenantAsync(
        FuelCardProviderType? providerType = null,
        CancellationToken ct = default);
}
