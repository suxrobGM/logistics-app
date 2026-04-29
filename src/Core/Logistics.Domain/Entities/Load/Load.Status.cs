using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Status transition methods for Load entity.
/// </summary>
public partial class Load
{
    /// <summary>
    /// Checks if transitioning to the specified status is allowed.
    /// </summary>
    public bool CanTransitionTo(LoadStatus next)
    {
        return LoadStatusMachine.CanTransition(Status, next);
    }

    /// <summary>
    /// Enforces valid status transitions and sets timestamps accordingly.
    /// </summary>
    /// <param name="newStatus">Target status.</param>
    /// <param name="force">
    /// If true, bypass transition validation (useful for data import/seeding).
    /// Still applies timestamp logic.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown for invalid transitions when <paramref name="force"/> is false.
    /// </exception>
    public void UpdateStatus(LoadStatus newStatus, bool force = false)
    {
        var timestamps = new LoadStatusTimestamps(
            DispatchedAt,
            PickedUpAt,
            DeliveredAt,
            CancelledAt);

        var result = LoadStatusMachine.Apply(Status, newStatus, force, timestamps);

        Status = result.Status;
        DispatchedAt = result.Timestamps.DispatchedAt;
        PickedUpAt = result.Timestamps.PickedUpAt;
        DeliveredAt = result.Timestamps.DeliveredAt;
        CancelledAt = result.Timestamps.CancelledAt;
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

        var statusToConfirm = Status switch
        {
            LoadStatus.Dispatched => (LoadStatus?)LoadStatus.PickedUp,
            LoadStatus.PickedUp => LoadStatus.Delivered,
            _ => null
        };

        if (statusToConfirm.HasValue)
        {
            RaiseProximityChangedEvent(statusToConfirm.Value);
        }
    }
}
