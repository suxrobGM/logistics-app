using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Routing;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class OptimizeTripStopsHandler :
    IAppRequestHandler<OptimizeTripStopsCommand, Result<OptimizedTripStopsDto>>
{
    private readonly ITripOptimizer _optimizer;
    private ILogger<OptimizeTripStopsHandler> _log;

    public OptimizeTripStopsHandler(ITripOptimizer optimizer, ILogger<OptimizeTripStopsHandler> log)
    {
        _optimizer = optimizer;
        _log = log;
    }

    public async Task<Result<OptimizedTripStopsDto>> Handle(OptimizeTripStopsCommand req, CancellationToken ct)
    {
        var stops = req.Stops.ToList();

        // Build request
        var optimizeTripParams = new OptimizeTripParams(
            stops,
            null,
            null, // optional
            req.MaxVehicles
        );

        // Optimize (Matrix or Heuristic fallback)
        var ordered = await _optimizer.OptimizeAsync(optimizeTripParams, ct);
        var totalDistance = ordered.Zip(ordered.Skip(1), (a, b) => a.Location.DistanceTo(b.Location)).Sum();

        var result = new OptimizedTripStopsDto
        {
            TotalDistance = totalDistance,
            OrderedStops = ordered.ToList()
        };
        return Result<OptimizedTripStopsDto>.Ok(result);
    }
}
