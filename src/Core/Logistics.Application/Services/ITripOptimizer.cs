using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Services;

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
