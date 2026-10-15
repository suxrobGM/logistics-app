using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Drayage data for the container's current move: steamship line, free time and rates, holds,
/// terminal appointments, chassis, empty return and street turn. Everything here except the line
/// and the rates belongs to one move and is cleared when a returned container starts the next one.
/// </summary>
public partial class Container
{
    /// <summary>Standard Carrier Alpha Code of the steamship line that owns the box (e.g. "MAEU").</summary>
    public string? SteamshipLineScac { get; set; }

    public string? SteamshipLineName { get; set; }

    /// <summary>
    /// Last day the box may sit at the terminal free of charge. Demurrage accrues for every
    /// calendar day after it until <see cref="PickedUpAt"/>.
    /// </summary>
    public DateOnly? LastFreeDay { get; set; }

    public decimal? DemurrageRatePerDay { get; set; }

    /// <summary>
    /// Last free day to return the empty to the line. Per diem accrues for every calendar day after
    /// it until <see cref="ReturnedAt"/> or <see cref="StreetTurnedAt"/>.
    /// </summary>
    public DateOnly? EmptyReturnDeadline { get; set; }

    public decimal? PerDiemRatePerDay { get; set; }

    /// <summary>
    /// When the box left the terminal (outgate). Stamped when the status leaves
    /// <see cref="ContainerStatus.AtPort"/>, editable by the dispatcher. Stops the demurrage clock
    /// and starts the chassis days.
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    public Guid? EmptyReturnTerminalId { get; set; }
    public virtual Terminal? EmptyReturnTerminal { get; set; }

    public bool CustomsHold { get; set; }
    public bool FreightHold { get; set; }
    public bool LineHold { get; set; }
    public bool OtherHold { get; set; }

    /// <summary>When the terminal lists the box as available for pickup.</summary>
    public DateTime? AvailableForPickupAt { get; set; }

    public string? PickupAppointmentNumber { get; set; }
    public DateTime? PickupAppointmentFrom { get; set; }
    public DateTime? PickupAppointmentTo { get; set; }

    public string? ReturnAppointmentNumber { get; set; }
    public DateTime? ReturnAppointmentFrom { get; set; }
    public DateTime? ReturnAppointmentTo { get; set; }

    public string? ChassisNumber { get; set; }
    public ChassisProvider? ChassisProvider { get; set; }
    public decimal? ChassisRatePerDay { get; set; }

    /// <summary>When the empty was street-turned to an export booking instead of being returned.</summary>
    public DateTime? StreetTurnedAt { get; private set; }

    /// <summary>The export booking the empty was street-turned to.</summary>
    public string? StreetTurnBookingReference { get; private set; }

    [NotMapped]
    public bool IsOnHold => CustomsHold || FreightHold || LineHold || OtherHold;

    public bool IsAvailableForPickup(DateTime now) =>
        !IsOnHold && AvailableForPickupAt is { } availableAt && availableAt <= now;

    public ContainerFreeTime GetFreeTime(DateTime now) => ContainerFreeTimeCalculator.Calculate(this, now);

    /// <summary>
    /// Records that the empty is being reused for an export booking instead of going back to the
    /// line. Stops the per-diem clock; the box stays <see cref="ContainerStatus.Empty"/>, ready to load.
    /// </summary>
    /// <exception cref="InvalidOperationException">The container is not empty.</exception>
    public void MarkStreetTurned(string exportBookingReference, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportBookingReference);

        if (Status != ContainerStatus.Empty || IsLaden)
        {
            throw new InvalidOperationException("Only an empty container can be street-turned.");
        }

        StreetTurnedAt = now;
        StreetTurnBookingReference = exportBookingReference.Trim();
    }

    /// <summary>
    /// Clears the per-move data when a returned box starts a new move, so the previous move's
    /// deadlines and holds do not raise alerts or charges against the next one.
    /// </summary>
    private void ResetMove()
    {
        LastFreeDay = null;
        EmptyReturnDeadline = null;
        PickedUpAt = null;
        EmptyReturnTerminalId = null;
        EmptyReturnTerminal = null;
        CustomsHold = false;
        FreightHold = false;
        LineHold = false;
        OtherHold = false;
        AvailableForPickupAt = null;
        PickupAppointmentNumber = null;
        PickupAppointmentFrom = null;
        PickupAppointmentTo = null;
        ReturnAppointmentNumber = null;
        ReturnAppointmentFrom = null;
        ReturnAppointmentTo = null;
        ChassisNumber = null;
        ChassisProvider = null;
        StreetTurnedAt = null;
        StreetTurnBookingReference = null;
    }
}
