using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.ApplicationServices;

/// <summary>Forward-looking margin for a load not yet hauled, for AI dispatch and the load board.</summary>
public interface IProfitEstimateService : IApplicationService
{
    /// <summary>Revenue minus operating cost minus the main driver's per-load pay; pairs with unknown costs are left out.</summary>
    Task<IReadOnlyDictionary<(Guid TruckId, Guid LoadId), decimal>> EstimateMarginsAsync(
        IReadOnlyCollection<MarginRequest> requests, CancellationToken ct = default);

    /// <summary>Fleet operating cost per mile, driver pay excluded; null when nothing is known.</summary>
    Task<decimal?> GetFleetCostPerMileAsync(CancellationToken ct = default);
}

/// <param name="TotalMeters">Deadhead plus loaded distance.</param>
public sealed record MarginRequest(Guid TruckId, Guid LoadId, double TotalMeters);
