using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class CertificationMapper
{
    public static DriverCertificationDto ToDto(this DriverCertification entity)
    {
        var daysUntilExpiration = (entity.ExpirationDate - DateTime.UtcNow).Days;

        return new DriverCertificationDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee?.GetFullName() ?? string.Empty,
            Type = entity.CertificationType,
            TypeDisplay = GetCertificationTypeDisplay(entity.CertificationType),
            Status = entity.Status,
            StatusDisplay = GetCertificationStatusDisplay(entity.Status),
            CertificationNumber = entity.CertificationNumber,
            IssuingAuthority = entity.IssuingAuthority,
            IssuingState = entity.IssuingState,
            IssueDate = entity.IssuedDate,
            ExpirationDate = entity.ExpirationDate,
            DaysUntilExpiration = daysUntilExpiration,
            CdlClass = entity.CdlClass,
            Endorsements = entity.Endorsements,
            Restrictions = entity.Restrictions,
            IsVerified = entity.IsVerified,
            VerifiedAt = entity.VerifiedAt,
            VerifiedById = entity.VerifiedById,
            VerifiedByName = entity.VerifiedBy?.GetFullName(),
            Notes = entity.Notes,
            Documents = entity.Documents.Select(d => d.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    public static TrainingRecordDto ToDto(this TrainingRecord entity)
    {
        int? daysUntilExpiration = entity.ExpirationDate.HasValue
            ? (entity.ExpirationDate.Value - DateTime.UtcNow).Days
            : null;

        return new TrainingRecordDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee?.GetFullName() ?? string.Empty,
            Type = entity.TrainingType,
            TypeDisplay = GetTrainingTypeDisplay(entity.TrainingType),
            CourseName = entity.TrainingName,
            Provider = entity.Provider,
            CompletedDate = entity.CompletedDate,
            ExpirationDate = entity.ExpirationDate,
            DaysUntilExpiration = daysUntilExpiration,
            Score = entity.Score,
            Passed = entity.IsPassed,
            CertificateNumber = entity.CertificateNumber,
            DurationHours = entity.Hours.HasValue ? (int?)entity.Hours.Value : null,
            Notes = entity.Notes,
            Documents = entity.Documents.Select(d => d.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    private static string GetCertificationTypeDisplay(CertificationType type)
    {
        return type switch
        {
            CertificationType.Cdl => "Commercial Driver's License (CDL)",
            CertificationType.MedicalCertificate => "DOT Medical Certificate",
            CertificationType.HazmatEndorsement => "Hazmat Endorsement",
            CertificationType.TwicCard => "TWIC Card",
            CertificationType.TankerEndorsement => "Tanker Endorsement",
            CertificationType.DoublesTriples => "Doubles/Triples Endorsement",
            CertificationType.PassengerEndorsement => "Passenger Endorsement",
            CertificationType.SchoolBusEndorsement => "School Bus Endorsement",
            CertificationType.Eldt => "Entry-Level Driver Training (ELDT)",
            CertificationType.DefensiveDriving => "Defensive Driving Certificate",
            CertificationType.HazmatTraining => "Hazmat Training Certificate",
            CertificationType.Other => "Other Certification",
            _ => "Unknown"
        };
    }

    private static string GetCertificationStatusDisplay(CertificationStatus status)
    {
        return status switch
        {
            CertificationStatus.Active => "Active",
            CertificationStatus.ExpiringSoon => "Expiring Soon",
            CertificationStatus.Expired => "Expired",
            CertificationStatus.Revoked => "Revoked",
            CertificationStatus.Suspended => "Suspended",
            _ => "Unknown"
        };
    }

    private static string GetTrainingTypeDisplay(TrainingType type)
    {
        return type switch
        {
            TrainingType.InitialHiring => "Initial Hiring Training",
            TrainingType.Annual => "Annual Refresher",
            TrainingType.Remedial => "Remedial Training",
            TrainingType.Safety => "Safety Training",
            TrainingType.Hazmat => "Hazmat Training",
            TrainingType.Defensive => "Defensive Driving",
            TrainingType.LoadSecurement => "Load Securement",
            TrainingType.CustomerService => "Customer Service",
            TrainingType.Equipment => "Equipment Training",
            TrainingType.Compliance => "Compliance Training",
            TrainingType.Eld => "ELD Training",
            TrainingType.Other => "Other Training",
            _ => "Unknown"
        };
    }
}
