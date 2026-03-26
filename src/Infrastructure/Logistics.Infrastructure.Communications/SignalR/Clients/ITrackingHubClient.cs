using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>
/// Hub client for geolocation live tracking and trip updates.
/// </summary>
public interface ITrackingHubClient
{
    /// <summary>
    /// Receives truck geolocation data updates.
    /// </summary>
    Task ReceiveGeolocationData(TruckGeolocationDto truckGeolocation);

    /// <summary>
    /// Receives trip status change notifications.
    /// </summary>
    Task ReceiveTripStatusUpdate(TripStatusUpdateDto statusUpdate);

    /// <summary>
    /// Receives stop arrival notifications.
    /// </summary>
    Task ReceiveStopArrival(StopArrivalUpdateDto stopArrival);

    /// <summary>
    /// Receives dispatch board updates (loads assigned/unassigned, trucks availability changes).
    /// </summary>
    Task ReceiveDispatchBoardUpdate(DispatchBoardUpdateDto update);

    /// <summary>
    /// Receives AI dispatch agent session updates (started, completed, failed).
    /// </summary>
    Task ReceiveDispatchAgentUpdate(DispatchAgentUpdateDto update);

    /// <summary>
    /// Receives individual AI dispatch agent decision notifications.
    /// </summary>
    Task ReceiveDispatchDecision(DispatchDecisionDto decision);
}
