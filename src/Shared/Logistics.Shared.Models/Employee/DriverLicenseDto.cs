using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class DriverLicenseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public LicenseClass LicenseClass { get; set; }
    public LicenseEndorsement[] Endorsements { get; set; } = [];
    public string IssuingCountry { get; set; } = null!;
    public string? IssuingRegion { get; set; }
    public DateOnly IssuedDate { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public DateOnly? MedicalCertExpiresAt { get; set; }
    public DriverLicenseStatus Status { get; set; }
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// Days remaining until expiry. Negative when already expired. Computed for display.
    /// </summary>
    public int DaysUntilExpiry { get; set; }
}
