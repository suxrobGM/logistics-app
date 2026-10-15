using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Common.Services;

/// <summary>
/// Compares a car hauler's <see cref="Truck.VehicleCapacity"/> with the vehicles on its open loads.
/// Over capacity is a warning for the dispatcher, never a block: what fits depends on the mix of
/// sedans and pickups, which a count cannot see.
/// </summary>
public interface IVehicleCapacityChecker : IApplicationService
{
    /// <summary>
    /// The warning to show, or null when the truck has no capacity set or is within it.
    /// </summary>
    /// <param name="truck">The car hauler.</param>
    /// <param name="candidate">A load being considered for the truck; counted even when not yet assigned.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string?> GetWarningAsync(Truck truck, Load? candidate = null, CancellationToken ct = default);
}
