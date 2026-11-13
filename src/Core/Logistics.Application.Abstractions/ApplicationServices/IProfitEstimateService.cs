using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.ApplicationServices;

/// <summary>Forward-looking margin for a load not yet hauled, for AI dispatch and the load board.</summary>
public interface IProfitEstimateService : IApplicationService
{
    /// <summary>
    /// Margin of each truck hauling each load: revenue minus operating cost over the total distance minus the
    /// truck's main driver's per-load pay. A pair is left out when the truck's operating cost is unknown.
    /// </summary>
    Task<IReadOnlyDictionary<(Guid TruckId, Guid LoadId), MarginEstimate>> EstimateMarginsAsync(
        IReadOnlyCollection<MarginRequest> requests, CancellationToken ct = default);

    /// <summary>Fleet operating cost per metre, driver pay excluded; null when nothing is known.</summary>
    Task<decimal?> GetFleetCostPerMetreAsync(CancellationToken ct = default);
}

/// <param name="TotalMeters">Deadhead plus loaded distance.</param>
public sealed record MarginRequest(Guid TruckId, Guid LoadId, double TotalMeters);

public sealed record MarginEstimate(decimal Revenue, decimal OperatingCost, decimal DriverPay)
{
    public decimal Margin => Revenue - OperatingCost - DriverPay;
}
