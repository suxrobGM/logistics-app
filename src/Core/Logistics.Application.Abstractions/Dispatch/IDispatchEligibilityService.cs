using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.Dispatch;

/// <summary>
/// Single source of truth for "can this truck + driver carry this load?". Validates driver
/// license status, endorsements, ADR/Hazmat compatibility, and truck ADR equipment. Used by
/// trip assignment commands and surfaced to the AI dispatch agent as a tool.
/// </summary>
public interface IDispatchEligibilityService : IApplicationService
{
    /// <summary>
    /// Checks whether the supplied truck (and optionally a specific driver) is eligible to
    /// carry the given load. Returns an <see cref="EligibilityResult"/> with structured
    /// reason codes — never throws on business-rule violations.
    /// </summary>
    /// <param name="truckId">Truck under consideration.</param>
    /// <param name="loadId">Target load.</param>
    /// <param name="driverId">
    /// Optional explicit driver. When <c>null</c>, the truck's current MainDriver is used.
    /// </param>
    Task<EligibilityResult> CheckAsync(
        Guid truckId,
        Guid loadId,
        Guid? driverId = null,
        CancellationToken ct = default);
}
