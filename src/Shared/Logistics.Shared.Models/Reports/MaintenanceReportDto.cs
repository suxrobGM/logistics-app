namespace Logistics.Shared.Models;

public record MaintenanceReportDto
{
    // Cost Summary
    public decimal TotalCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }

    // Service Summary
    public int TotalServices { get; set; }
    public int ScheduledServices { get; set; }
    public int UnscheduledServices { get; set; }

    // Schedule Summary
    public int TotalSchedules { get; set; }
    public int OverdueCount { get; set; }
    public int DueSoonCount { get; set; }

    // Breakdowns
    public IReadOnlyList<MaintenanceTypeBreakdownDto> ByServiceType { get; set; } = [];
    public IReadOnlyList<MaintenanceVendorBreakdownDto> ByVendor { get; set; } = [];

    // Trends
    public IReadOnlyList<MaintenanceTrendDto> CostTrends { get; set; } = [];
    public IReadOnlyList<MaintenanceTrendDto> ServiceTrends { get; set; } = [];

    // Top cost trucks
    public IReadOnlyList<MaintenanceTruckSummaryDto> TopCostTrucks { get; set; } = [];
}

public record MaintenanceTypeBreakdownDto
{
    public string MaintenanceType { get; set; } = string.Empty;
    public string MaintenanceTypeDisplay { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
}

public record MaintenanceVendorBreakdownDto
{
    public string VendorName { get; set; } = string.Empty;
    public int ServiceCount { get; set; }
    public decimal TotalCost { get; set; }
}

public record MaintenanceTrendDto
{
    public string Period { get; set; } = string.Empty;
    public int ServiceCount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
}

public record MaintenanceTruckSummaryDto
{
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;
    public int ServiceCount { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime? LastServiceDate { get; set; }
}
