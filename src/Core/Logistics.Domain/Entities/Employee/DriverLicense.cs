using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Per-driver license record. A single Employee can hold multiple licenses over time
/// (history of issued / renewed / superseded), and across jurisdictions (e.g., a driver
/// who holds both a US CDL and an EU CE).
/// </summary>
public class DriverLicense : AuditableEntity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required string LicenseNumber { get; set; }
    public required LicenseClass LicenseClass { get; set; }

    /// <summary>
    /// Bitfield of <see cref="LicenseEndorsement"/> values held on this license.
    /// </summary>
    public LicenseEndorsement Endorsements { get; set; } = LicenseEndorsement.None;

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "DE", "PL").
    /// </summary>
    public required string IssuingCountry { get; set; }

    /// <summary>
    /// State / province / region within the issuing country (e.g., "TX", "BY", "Île-de-France").
    /// </summary>
    public string? IssuingRegion { get; set; }

    public DateTime IssuedDate { get; set; }
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// US DOT medical card expiry. Applies only to US CDL holders.
    /// </summary>
    public DateTime? MedicalCertExpiresAt { get; set; }

    public DriverLicenseStatus Status { get; set; } = DriverLicenseStatus.Active;

    /// <summary>
    /// Optional FK to an <see cref="EmployeeDocument"/> holding the scanned license image / PDF.
    /// </summary>
    public Guid? DocumentId { get; set; }

    public virtual EmployeeDocument? Document { get; set; }

    /// <summary>
    /// The threshold (60, 30, 7) of the most recent expiry reminder, so the job sends each one once.
    /// </summary>
    public int? LastReminderThresholdDays { get; private set; }

    /// <summary>
    /// Sets the expiry. A new expiry (a renewal or a correction) restarts the expiry reminders.
    /// </summary>
    public void SetExpiry(DateTime expiresAt)
    {
        if (expiresAt == ExpiresAt)
        {
            return;
        }

        ExpiresAt = expiresAt;
        LastReminderThresholdDays = null;
    }

    public void RecordReminderSent(int thresholdDays) => LastReminderThresholdDays = thresholdDays;

    public bool IsExpired(DateTime utcNow) =>
        Status == DriverLicenseStatus.Expired || ExpiresAt <= utcNow;

    public bool HasEndorsement(LicenseEndorsement endorsement) =>
        Endorsements.HasFlag(endorsement);

    public static DriverLicense Create(
        Guid employeeId,
        string licenseNumber,
        LicenseClass licenseClass,
        string issuingCountry,
        DateTime issuedDate,
        DateTime expiresAt,
        LicenseEndorsement endorsements = LicenseEndorsement.None,
        string? issuingRegion = null,
        DateTime? medicalCertExpiresAt = null,
        Guid? documentId = null)
    {
        var license = new DriverLicense
        {
            EmployeeId = employeeId,
            LicenseNumber = licenseNumber,
            LicenseClass = licenseClass,
            Endorsements = endorsements,
            IssuingCountry = issuingCountry,
            IssuingRegion = issuingRegion,
            IssuedDate = issuedDate,
            ExpiresAt = expiresAt,
            MedicalCertExpiresAt = medicalCertExpiresAt,
            DocumentId = documentId,
            Status = DriverLicenseStatus.Active
        };

        license.DomainEvents.Add(new DriverLicenseCreatedEvent(license.Id, employeeId));
        return license;
    }
}
