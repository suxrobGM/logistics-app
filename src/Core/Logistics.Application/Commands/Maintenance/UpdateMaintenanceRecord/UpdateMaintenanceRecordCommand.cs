using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Maintenance;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record UpdateMaintenanceRecordCommand : IAppRequest<Result<MaintenanceRecordDto>>
{
    public required Guid Id { get; set; }
    public required Guid TruckId { get; set; }
    public required MaintenanceType Type { get; set; }
    public required string Description { get; set; }
    public required DateTime ServiceDate { get; set; }
    public int? OdometerReading { get; set; }
    public int? EngineHours { get; set; }
    public string? VendorName { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public Guid? PerformedById { get; set; }
    public string? Notes { get; set; }
}
