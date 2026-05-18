using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class UpdateDriverLicenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateDriverLicenseCommand, Result>
{
    public async Task<Result> Handle(UpdateDriverLicenseCommand req, CancellationToken ct)
    {
        var license = await tenantUow.Repository<DriverLicense>().GetByIdAsync(req.LicenseId, ct);
        if (license is null)
        {
            return Result.Fail($"Driver license not found with ID '{req.LicenseId}'");
        }

        if (req.DocumentId.HasValue)
        {
            var doc = await tenantUow.Repository<EmployeeDocument>().GetByIdAsync(req.DocumentId.Value, ct);
            if (doc is null)
            {
                return Result.Fail($"Document '{req.DocumentId}' not found");
            }
            if (doc.EmployeeId != license.EmployeeId)
            {
                return Result.Fail(
                    $"Document '{req.DocumentId}' belongs to a different employee and cannot be attached to this license");
            }
        }

        license.LicenseClass = PropertyUpdater.UpdateIfChanged(req.LicenseClass, license.LicenseClass);
        if (req.Endorsements is not null)
        {
            license.Endorsements = req.Endorsements.Combine();
        }
        license.IssuingRegion = PropertyUpdater.UpdateIfChanged(req.IssuingRegion, license.IssuingRegion);
        license.IssuedDate = PropertyUpdater.UpdateIfChanged(req.IssuedDate, license.IssuedDate);
        license.ExpiresAt = PropertyUpdater.UpdateIfChanged(req.ExpiresAt, license.ExpiresAt);
        license.Status = PropertyUpdater.UpdateIfChanged(req.Status, license.Status);

        // Nullable-struct targets — PropertyUpdater has no overload for these.
        if (req.MedicalCertExpiresAt.HasValue)
        {
            license.MedicalCertExpiresAt = req.MedicalCertExpiresAt;
        }
        if (req.DocumentId.HasValue)
        {
            license.DocumentId = req.DocumentId;
        }

        if (license.ExpiresAt <= license.IssuedDate)
        {
            return Result.Fail("Expiry must be after issued date.");
        }

        tenantUow.Repository<DriverLicense>().Update(license);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
