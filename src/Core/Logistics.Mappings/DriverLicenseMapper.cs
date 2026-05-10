using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class DriverLicenseMapper
{
    public static DriverLicenseDto ToDto(this DriverLicense entity)
    {
        return new DriverLicenseDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            LicenseNumber = entity.LicenseNumber,
            LicenseClass = entity.LicenseClass,
            Endorsements = entity.Endorsements.ToArray(),
            IssuingCountry = entity.IssuingCountry,
            IssuingRegion = entity.IssuingRegion,
            IssuedDate = entity.IssuedDate,
            ExpiresAt = entity.ExpiresAt,
            MedicalCertExpiresAt = entity.MedicalCertExpiresAt,
            Status = entity.Status,
            DocumentId = entity.DocumentId,
            DaysUntilExpiry = (int)(entity.ExpiresAt - DateTime.UtcNow).TotalDays
        };
    }
}
