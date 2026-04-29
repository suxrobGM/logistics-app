using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateContainerCommand : IAppRequest<Result<ContainerDto>>
{
    public string Number { get; set; } = null!;
    public ContainerIsoType IsoType { get; set; }
    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public bool IsLaden { get; set; }
    public decimal GrossWeight { get; set; }
    public ContainerStatus Status { get; set; } = ContainerStatus.Empty;
    public Guid? CurrentTerminalId { get; set; }
    public string? Notes { get; set; }
}
