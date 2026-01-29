using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class DvirReportDto
{
    public Guid Id { get; set; }
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;

    public DvirType Type { get; set; }
    public DvirStatus Status { get; set; }
    public DateTime InspectionDate { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? OdometerReading { get; set; }

    public bool HasDefects { get; set; }
    public bool HasDriverSignature { get; set; }
    public string? DriverNotes { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool HasMechanicSignature { get; set; }
    public string? MechanicNotes { get; set; }
    public bool? DefectsCorrected { get; set; }

    public Guid? TripId { get; set; }

    public List<DvirDefectDto> Defects { get; set; } = [];
    public List<DocumentDto> Photos { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class DvirDefectDto
{
    public Guid Id { get; set; }
    public DvirInspectionCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public string SeverityDisplay { get; set; } = string.Empty;
    public bool IsCorrected { get; set; }
    public string? CorrectionNotes { get; set; }
    public DateTime? CorrectedAt { get; set; }
    public Guid? CorrectedById { get; set; }
    public string? CorrectedByName { get; set; }
}

public class DvirSummaryDto
{
    public int TotalReports { get; set; }
    public int PendingReviews { get; set; }
    public int RequiringRepairs { get; set; }
    public int ClearedToday { get; set; }
    public int DefectsFoundThisWeek { get; set; }
}

#region Request DTOs

/// <summary>
/// Request DTO for creating a defect within a DVIR report
/// </summary>
public class CreateDvirDefectRequest
{
    public required DvirInspectionCategory Category { get; set; }
    public required string Description { get; set; }
    public required DefectSeverity Severity { get; set; }
}

#endregion
