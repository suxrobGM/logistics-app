using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Routing;

namespace Logistics.Application.Abstractions.Routing;

/// <summary>
///     Service for optimizing trip routes.
/// </summary>
public interface ITripOptimizer
{
    Task<IReadOnlyList<TripStopDto>> OptimizeAsync(OptimizeTripParams @params, CancellationToken ct = default);
}

public record OptimizeTripParams(
    IReadOnlyList<TripStopDto> Stops,
    GeoPoint? Start,
    GeoPoint? End,
    int VehicleCapacity);
