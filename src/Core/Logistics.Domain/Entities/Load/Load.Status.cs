using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Status transition methods for Load entity.
/// </summary>
public partial class Load
{

    /// <summary>
    /// Enforces valid status transitions and sets timestamps accordingly.
    /// Raises <see cref="LoadStatusChangedEvent"/> when the status actually changes.
    /// </summary>
    /// <param name="newStatus">Target status.</param>
    /// <exception cref="InvalidOperationException">Thrown for invalid transitions.</exception>
    public void UpdateStatus(LoadStatus newStatus)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new InvalidOperationException($"Cannot change load status from '{Status}' to '{newStatus}'.");
        }

        var oldStatus = Status;
        var now = DateTime.UtcNow;
        switch (newStatus)
        {
            case LoadStatus.Dispatched:
                // A re-dispatch keeps the first dispatch time but starts the run over.
                DispatchedAt ??= now;
                PickedUpAt = null;
                DeliveredAt = null;
                CancelledAt = null;
                break;
            case LoadStatus.PickedUp:
                PickedUpAt ??= now;
                break;
            case LoadStatus.Delivered:
                DeliveredAt ??= now;
                break;
            case LoadStatus.Cancelled:
                CancelledAt ??= now;
                break;
        }

        Status = newStatus;
        DomainEvents.Add(new LoadStatusChangedEvent(Id, oldStatus, Status));
    }

    private static readonly Dictionary<LoadStatus, LoadStatus[]> AllowedTransitions = new()
    {
        [LoadStatus.Draft] = [LoadStatus.Dispatched, LoadStatus.Cancelled],
        [LoadStatus.Dispatched] = [LoadStatus.PickedUp, LoadStatus.Cancelled],
        [LoadStatus.PickedUp] = [LoadStatus.Delivered, LoadStatus.Cancelled],
        [LoadStatus.Delivered] = [],
        [LoadStatus.Cancelled] = []
    };

    /// <summary>
    /// Sets the status of a load imported from another system, dating each milestone from the source
    /// rather than now, and raising no events: history must not notify drivers.
    /// </summary>
    public void RestoreImportedStatus(LoadStatus status, DateTime? pickedUpAt, DateTime? deliveredAt)
    {
        var now = DateTime.UtcNow;
        Status = status;
        DispatchedAt = status is LoadStatus.Draft or LoadStatus.Cancelled ? null : pickedUpAt ?? deliveredAt ?? now;
        PickedUpAt = status is LoadStatus.PickedUp or LoadStatus.Delivered ? pickedUpAt ?? deliveredAt ?? now : null;
        DeliveredAt = status is LoadStatus.Delivered ? deliveredAt ?? pickedUpAt ?? now : null;
        CancelledAt = status is LoadStatus.Cancelled ? deliveredAt ?? pickedUpAt ?? now : null;
    }

    /// <summary>
    /// Dispatches the load to the assigned truck.
    /// Also sets the invoice to Issued status if it's in Draft.
    /// </summary>
    public void Dispatch(DateTime? at = null)
    {
        if (at.HasValue)
        {
            DispatchedAt = at.Value;
        }

        UpdateStatus(LoadStatus.Dispatched);

        // Set invoice to issued when load is dispatched
        if (Invoice is { Status: InvoiceStatus.Draft })
        {
            Invoice.Status = InvoiceStatus.Issued;
        }
    }

    /// <summary>
    /// Refreshes the status of each trip that carries this load. Call it after a status change outside a trip.
    /// </summary>
    public void RefreshTripStatus()
    {
        foreach (var trip in TripStops.Select(s => s.Trip).Distinct())
        {
            trip.RefreshStatus();
        }
    }

    /// <summary>
    /// Confirms pickup of the load.
    /// </summary>
    public void ConfirmPickup(DateTime? at = null)
    {
        if (at.HasValue)
        {
            PickedUpAt = at.Value;
        }

        UpdateStatus(LoadStatus.PickedUp);
    }

    /// <summary>
    /// Confirms delivery of the load.
    /// </summary>
    public void ConfirmDelivery(DateTime? at = null)
    {
        if (at.HasValue)
        {
            DeliveredAt = at.Value;
        }

        UpdateStatus(LoadStatus.Delivered);
    }

    /// <summary>
    /// Cancels the load.
    /// </summary>
    public void Cancel()
    {
        UpdateStatus(LoadStatus.Cancelled);
    }

    /// <summary>
    /// Updates the proximity flag based on whether the truck is inside the next
    /// confirmation checkpoint's geofence. Raises <see cref="LoadProximityChangedEvent"/>
    /// when proximity becomes true and a confirmation action is now possible.
    /// The "next confirmation" is implied by the current <see cref="Status"/>
    /// (pickup if Dispatched, delivery if PickedUp).
    /// </summary>
    public void UpdateProximity(bool isInProximity)
    {
        if (isInProximity == IsInProximity)
        {
            return;
        }

        IsInProximity = isInProximity;

        if (!isInProximity)
        {
            return;
        }

        if (NextDriverStatus is { } statusToConfirm)
        {
            RaiseProximityChangedEvent(statusToConfirm);
        }
    }
}
