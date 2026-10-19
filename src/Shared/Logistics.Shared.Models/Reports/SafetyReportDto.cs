namespace Logistics.Shared.Models;

public record SafetyReportDto
{
    // DVIR Summary
    public int TotalDvirs { get; set; }
    public int PendingDvirReviews { get; set; }
    public int DvirsWithDefects { get; set; }
    public double DvirDefectRate { get; set; }

    // Accident Summary
    public int TotalAccidents { get; set; }
    public int UnresolvedAccidents { get; set; }
    public decimal EstimatedDamageCost { get; set; }
    public int InjuriesReported { get; set; }

    // Breakdowns
    public IReadOnlyList<SafetyStatusBreakdownDto> DvirStatusBreakdown { get; set; } = [];
    public IReadOnlyList<SafetyStatusBreakdownDto> AccidentStatusBreakdown { get; set; } = [];
    public IReadOnlyList<SafetySeverityBreakdownDto> AccidentSeverityBreakdown { get; set; } = [];

    // Trends
    public IReadOnlyList<SafetyTrendDto> DvirTrends { get; set; } = [];
    public IReadOnlyList<SafetyTrendDto> AccidentTrends { get; set; } = [];

    // Top offenders
    public IReadOnlyList<SafetyDriverSummaryDto> TopDriversByAccidents { get; set; } = [];
    public IReadOnlyList<SafetyTruckSummaryDto> TopTrucksByDefects { get; set; } = [];
}

public record SafetyStatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public int Count { get; set; }
}

public record SafetySeverityBreakdownDto
{
    public string Severity { get; set; } = string.Empty;
    public string SeverityDisplay { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalDamage { get; set; }
}

public record SafetyTrendDto
{
    public string Period { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal? Value { get; set; }
}

public record SafetyDriverSummaryDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int AccidentCount { get; set; }
}

public record SafetyTruckSummaryDto
{
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;
    public int DefectCount { get; set; }
    public int AccidentCount { get; set; }
}
