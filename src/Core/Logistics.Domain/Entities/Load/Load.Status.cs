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
    /// Enforces valid status transitions and sets timestamps/flags accordingly.
    /// </summary>
    /// <param name="newStatus">Target status.</param>
    /// <param name="force">
    /// If true, bypass transition validation (useful for data import/seeding).
    /// Still applies timestamp/flag logic.
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
            CancelledAt,
            CanConfirmPickUp,
            CanConfirmDelivery);

        var result = LoadStatusMachine.Apply(Status, newStatus, force, timestamps);

        Status = result.Status;
        CanConfirmPickUp = result.CanConfirmPickUp;
        CanConfirmDelivery = result.CanConfirmDelivery;
        DispatchedAt = result.Timestamps.DispatchedAt;
        PickedUpAt = result.Timestamps.PickedUpAt;
        DeliveredAt = result.Timestamps.DeliveredAt;
        CancelledAt = result.Timestamps.CancelledAt;
    }

    /// <summary>
    /// Dispatches the load to the assigned truck.
    /// Also sets all draft invoices to Issued status.
    /// </summary>
    public void Dispatch(DateTime? at = null)
    {
        if (at.HasValue)
        {
            DispatchedAt = at.Value;
        }

        UpdateStatus(LoadStatus.Dispatched);

        // Set all draft invoices to issued when load is dispatched
        foreach (var invoice in Invoices.Where(i => i.Status == InvoiceStatus.Draft))
        {
            invoice.Status = InvoiceStatus.Issued;
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
    /// Updates the proximity status and raises the LoadProximityChangedEvent for notifications.
    /// </summary>
    public void UpdateProximity(bool? canConfirmPickUp, bool? canConfirmDelivery)
    {
        LoadStatus? statusToConfirm = null;

        if (canConfirmPickUp.HasValue && canConfirmPickUp != CanConfirmPickUp)
        {
            CanConfirmPickUp = canConfirmPickUp.Value;
            if (canConfirmPickUp.Value)
            {
                statusToConfirm = LoadStatus.PickedUp;
            }
        }

        if (canConfirmDelivery.HasValue && canConfirmDelivery != CanConfirmDelivery)
        {
            CanConfirmDelivery = canConfirmDelivery.Value;
            if (canConfirmDelivery.Value)
            {
                statusToConfirm = LoadStatus.Delivered;
            }
        }

        if (statusToConfirm.HasValue)
        {
            RaiseProximityChangedEvent(statusToConfirm.Value);
        }
    }
}
