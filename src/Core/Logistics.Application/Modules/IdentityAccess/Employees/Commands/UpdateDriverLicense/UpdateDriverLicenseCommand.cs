using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class UpdateDriverLicenseCommand : ICommand
{
    public Guid LicenseId { get; set; }
    public LicenseClass? LicenseClass { get; set; }
    public LicenseEndorsement[]? Endorsements { get; set; }
    public string? IssuingRegion { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public DateOnly? MedicalCertExpiresAt { get; set; }
    public DriverLicenseStatus? Status { get; set; }
    public Guid? DocumentId { get; set; }
}
