namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Retries a trip alert turn whose conversation was mid-turn when the alert arrived.
/// </summary>
public record TripAlertWakeRequest(Guid TenantId, Guid TripId);
