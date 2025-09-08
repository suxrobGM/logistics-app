using Logistics.Application.Services;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services.Trip;

public sealed class CompositeTripOptimizer : ITripOptimizer
{
    private readonly HeuristicTripOptimizer _heuristic;
    private readonly ILogger<CompositeTripOptimizer> _log;
    private readonly MapboxMatrixTripOptimizer _matrixOpt;

    public CompositeTripOptimizer(MapboxMatrixTripOptimizer matrixOpt, HeuristicTripOptimizer heuristic,
        ILogger<CompositeTripOptimizer> log)
    {
        _matrixOpt = matrixOpt;
        _heuristic = heuristic;
        _log = log;
    }

    public async Task<IReadOnlyList<TripStopDto>> OptimizeAsync(OptimizeTripParams @params,
        CancellationToken ct = default)
    {
        try
        {
            _log.LogInformation("Using Mapbox Matrix API for trip optimization");
            return await _matrixOpt.OptimizeAsync(@params, ct);
        }
        catch
        {
            // fall back to local heuristic
            _log.LogInformation("Using Heuristic API for trip optimization");
            return await _heuristic.OptimizeAsync(@params, ct);
        }
    }
}
