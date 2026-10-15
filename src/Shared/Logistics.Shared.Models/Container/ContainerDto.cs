using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class ContainerDto
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public ContainerIsoType IsoType { get; set; }
    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public bool IsLaden { get; set; }
    public decimal GrossWeight { get; set; }
    public ContainerStatus Status { get; set; }
    public Guid? CurrentTerminalId { get; set; }
    public string? CurrentTerminalName { get; set; }
    public string? CurrentTerminalCode { get; set; }
    public string? Notes { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? SteamshipLineScac { get; set; }
    public string? SteamshipLineName { get; set; }
    public DateOnly? LastFreeDay { get; set; }
    public decimal? DemurrageRatePerDay { get; set; }
    public DateOnly? EmptyReturnDeadline { get; set; }
    public decimal? PerDiemRatePerDay { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public Guid? EmptyReturnTerminalId { get; set; }
    public string? EmptyReturnTerminalName { get; set; }
    public string? EmptyReturnTerminalCode { get; set; }
    public bool CustomsHold { get; set; }
    public bool FreightHold { get; set; }
    public bool LineHold { get; set; }
    public bool OtherHold { get; set; }
    public bool IsOnHold { get; set; }
    public DateTime? AvailableForPickupAt { get; set; }
    public bool IsAvailableForPickup { get; set; }
    public string? PickupAppointmentNumber { get; set; }
    public DateTime? PickupAppointmentFrom { get; set; }
    public DateTime? PickupAppointmentTo { get; set; }
    public string? ReturnAppointmentNumber { get; set; }
    public DateTime? ReturnAppointmentFrom { get; set; }
    public DateTime? ReturnAppointmentTo { get; set; }
    public string? ChassisNumber { get; set; }
    public ChassisProvider? ChassisProvider { get; set; }
    public decimal? ChassisRatePerDay { get; set; }
    public DateTime? StreetTurnedAt { get; set; }
    public string? StreetTurnBookingReference { get; set; }

    public FreeTimeClockDto Demurrage { get; set; } = new();
    public FreeTimeClockDto PerDiem { get; set; } = new();
    public ChassisUsageDto Chassis { get; set; } = new();
    public decimal TotalCharge { get; set; }
}
