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
}
