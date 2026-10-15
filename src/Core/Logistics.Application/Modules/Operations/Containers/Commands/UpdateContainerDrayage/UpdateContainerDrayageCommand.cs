using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Replaces the container's drayage data (steamship line, free time and rates, holds, appointments,
/// chassis, empty return). A full replace rather than a patch, so a dispatcher can clear a value.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class UpdateContainerDrayageCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public string? SteamshipLineScac { get; set; }
    public string? SteamshipLineName { get; set; }
    public DateOnly? LastFreeDay { get; set; }
    public decimal? DemurrageRatePerDay { get; set; }
    public DateOnly? EmptyReturnDeadline { get; set; }
    public decimal? PerDiemRatePerDay { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public Guid? EmptyReturnTerminalId { get; set; }
    public bool CustomsHold { get; set; }
    public bool FreightHold { get; set; }
    public bool LineHold { get; set; }
    public bool OtherHold { get; set; }
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
}
