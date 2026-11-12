using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DriverLicenseMapper
{
    [MapperIgnoreSource(nameof(DriverLicense.Employee))]
    [MapperIgnoreSource(nameof(DriverLicense.Document))]
    [MapProperty(nameof(DriverLicense.Endorsements), nameof(DriverLicenseDto.Endorsements), Use = nameof(ExpandEndorsements))]
    [MapProperty(nameof(DriverLicense.ExpiresAt), nameof(DriverLicenseDto.DaysUntilExpiry), Use = nameof(CountDaysUntil))]
    public static partial DriverLicenseDto ToDto(this DriverLicense entity);

    private static LicenseEndorsement[] ExpandEndorsements(LicenseEndorsement endorsements) => endorsements.ToArray();

    private static int CountDaysUntil(DateOnly date) => date.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
}
