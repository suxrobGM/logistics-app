using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record HosViolationDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime ViolationDate { get; set; }
    public HosViolationType ViolationType { get; set; }
    public string? ViolationTypeDisplay { get; set; }
    public string? Description { get; set; }
    public int SeverityLevel { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public EldProviderType ProviderType { get; set; }
}

/// <summary>
/// Summary of violations for reporting
/// </summary>
public record ViolationSummaryDto
{
    public int TotalViolations { get; set; }
    public int UnresolvedViolations { get; set; }
    public int ResolvedViolations { get; set; }
    public Dictionary<string, int> ViolationsByType { get; set; } = [];
    public List<HosViolationDto> RecentViolations { get; set; } = [];
}
