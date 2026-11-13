using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Employees.Commands;

/// <summary>
/// Soft-deletes a driver license by transitioning its status to <see cref="DriverLicenseStatus.Revoked"/>.
/// Hard delete is intentionally not supported - license history must be retained for audits.
/// </summary>
internal sealed class DeleteDriverLicenseHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<DeleteDriverLicenseCommand, Result>
{
    public async Task<Result> Handle(DeleteDriverLicenseCommand req, CancellationToken ct)
    {
        var license = await tenantUow.Repository<DriverLicense>().GetByIdAsync(req.LicenseId, ct);
        if (license is null)
        {
            return Result.Fail($"Driver license not found with ID '{req.LicenseId}'", ErrorCodes.NotFound);
        }

        license.Status = DriverLicenseStatus.Revoked;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
