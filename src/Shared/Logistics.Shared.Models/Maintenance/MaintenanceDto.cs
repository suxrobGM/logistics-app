using Logistics.Domain.Primitives.Enums.Maintenance;

namespace Logistics.Shared.Models;

public class MaintenanceRecordDto
{
    public Guid Id { get; set; }
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;

    public MaintenanceType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime ServiceDate { get; set; }
    public int? OdometerReading { get; set; }
    public int? EngineHours { get; set; }

    public string? VendorName { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }

    public Guid? PerformedById { get; set; }
    public string? PerformedByName { get; set; }

    public string? Notes { get; set; }
    public List<MaintenancePartDto> Parts { get; set; } = [];
    public List<DocumentDto> Documents { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class MaintenancePartDto
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Vendor { get; set; }
    public string? WarrantyInfo { get; set; }
}

#region Request DTOs

public class CreateMaintenanceRecordRequest
{
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
    public List<CreateMaintenancePartRequest> Parts { get; set; } = [];
}

public class CreateMaintenancePartRequest
{
    public required string PartNumber { get; set; }
    public required string PartName { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitCost { get; set; }
    public string? Vendor { get; set; }
    public string? WarrantyInfo { get; set; }
}

#endregion
