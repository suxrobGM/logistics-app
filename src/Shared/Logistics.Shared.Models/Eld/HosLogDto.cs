using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record HosLogDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime LogDate { get; set; }
    public DutyStatus DutyStatus { get; set; }
    public string? DutyStatusDisplay { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? DurationDisplay { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Remark { get; set; }
    public EldProviderType ProviderType { get; set; }
}

/// <summary>
/// Daily summary of HOS logs grouped by date
/// </summary>
public record HosDailySummaryDto
{
    public DateTime Date { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int TotalDrivingMinutes { get; set; }
    public int TotalOnDutyMinutes { get; set; }
    public int TotalSleeperMinutes { get; set; }
    public int TotalOffDutyMinutes { get; set; }
    public List<HosLogDto> Logs { get; set; } = [];
}
