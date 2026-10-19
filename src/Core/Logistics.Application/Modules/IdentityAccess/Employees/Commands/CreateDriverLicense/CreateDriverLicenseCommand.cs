using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class CreateDriverLicenseCommand : ICommand<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public LicenseClass LicenseClass { get; set; }
    public LicenseEndorsement[] Endorsements { get; set; } = [];
    public string IssuingCountry { get; set; } = null!;
    public string? IssuingRegion { get; set; }
    public DateOnly IssuedDate { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public DateOnly? MedicalCertExpiresAt { get; set; }
    public Guid? DocumentId { get; set; }
}
