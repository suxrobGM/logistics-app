using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class DriverCertificationDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    public CertificationType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public CertificationStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;

    public string CertificationNumber { get; set; } = string.Empty;
    public string? IssuingAuthority { get; set; }
    public string? IssuingState { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int DaysUntilExpiration { get; set; }

    public CdlClass? CdlClass { get; set; }
    public string? Endorsements { get; set; }
    public string? Restrictions { get; set; }

    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedById { get; set; }
    public string? VerifiedByName { get; set; }

    public string? Notes { get; set; }
    public List<DocumentDto> Documents { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class TrainingRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    public TrainingType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? Provider { get; set; }

    public DateTime CompletedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? DaysUntilExpiration { get; set; }

    public decimal? Score { get; set; }
    public bool Passed { get; set; }
    public string? CertificateNumber { get; set; }

    public int? DurationHours { get; set; }
    public string? Notes { get; set; }
    public List<DocumentDto> Documents { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class CertificationSummaryDto
{
    public int TotalCertifications { get; set; }
    public int ActiveCertifications { get; set; }
    public int ExpiringSoon { get; set; }
    public int Expired { get; set; }
    public int PendingVerification { get; set; }
}

#region Request DTOs

public class CreateDriverCertificationRequest
{
    public required Guid EmployeeId { get; set; }
    public required CertificationType Type { get; set; }
    public required string CertificationNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? IssuingState { get; set; }
    public required DateTime IssueDate { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public CdlClass? CdlClass { get; set; }
    public string? Endorsements { get; set; }
    public string? Restrictions { get; set; }
    public string? Notes { get; set; }
}

public class CreateTrainingRecordRequest
{
    public required Guid EmployeeId { get; set; }
    public required TrainingType Type { get; set; }
    public required string CourseName { get; set; }
    public string? Provider { get; set; }
    public required DateTime CompletedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal? Score { get; set; }
    public required bool Passed { get; set; }
    public string? CertificateNumber { get; set; }
    public int? DurationHours { get; set; }
    public string? Notes { get; set; }
}

#endregion
