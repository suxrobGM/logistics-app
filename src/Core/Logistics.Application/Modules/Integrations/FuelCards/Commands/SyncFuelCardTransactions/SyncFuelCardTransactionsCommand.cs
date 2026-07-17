using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

/// <summary>
/// Manually triggers a fuel card transaction sync for the current tenant
/// (the nightly Hangfire job runs the same logic).
/// </summary>
[RequiresFeature(TenantFeature.FuelCards)]
public class SyncFuelCardTransactionsCommand : ICommand<Result<FuelCardSyncResultDto>>
{
    /// <summary>
    /// Sync only this provider; null syncs all active providers.
    /// </summary>
    public FuelCardProviderType? ProviderType { get; set; }
}
