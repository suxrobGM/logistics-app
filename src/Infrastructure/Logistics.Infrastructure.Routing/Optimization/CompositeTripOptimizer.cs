using Logistics.Application.Services;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Routing.Optimization;

internal sealed class CompositeTripOptimizer(
    MapboxMatrixTripOptimizer matrixOpt,
    HeuristicTripOptimizer heuristic,
    ILogger<CompositeTripOptimizer> logger)
    : ITripOptimizer
{
    public async Task<IReadOnlyList<TripStopDto>> OptimizeAsync(OptimizeTripParams @params,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Using Mapbox Matrix API for trip optimization");
            return await matrixOpt.OptimizeAsync(@params, ct);
        }
        catch
        {
            // fall back to local heuristic
            logger.LogInformation("Using Heuristic API for trip optimization");
            return await heuristic.OptimizeAsync(@params, ct);
        }
    }
}
