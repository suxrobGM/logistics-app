namespace Logistics.Application.Abstractions.Dispatch;

/// <summary>
/// Narrow port for "can this truck + driver carry this load?" used by Infrastructure-side
/// consumers (notably the AI dispatch agent's tool layer) that should not depend on the
/// full <c>IDispatchEligibilityService</c> defined in the Application layer.
/// </summary>
public interface IEligibilityCheck
{
    Task<EligibilityResult> CheckAsync(
        Guid truckId,
        Guid loadId,
        Guid? driverId = null,
        CancellationToken ct = default);
}
