using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateDriverLicenseCommand : IAppRequest
{
    public Guid LicenseId { get; set; }
    public LicenseClass? LicenseClass { get; set; }
    public LicenseEndorsement[]? Endorsements { get; set; }
    public string? IssuingRegion { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? MedicalCertExpiresAt { get; set; }
    public DriverLicenseStatus? Status { get; set; }
    public Guid? DocumentId { get; set; }
}
