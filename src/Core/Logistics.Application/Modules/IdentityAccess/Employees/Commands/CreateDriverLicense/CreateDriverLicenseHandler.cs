using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class CreateDriverLicenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateDriverLicenseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDriverLicenseCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);
        if (employee is null)
        {
            return Result<Guid>.Fail($"Employee not found with ID '{req.EmployeeId}'");
        }

        // Reject duplicate (employee, license number, country) combinations.
        var duplicate = await tenantUow.Repository<DriverLicense>().GetAsync(
            i => i.EmployeeId == req.EmployeeId
                 && i.LicenseNumber == req.LicenseNumber
                 && i.IssuingCountry == req.IssuingCountry,
            ct);
        if (duplicate is not null)
        {
            return Result<Guid>.Fail(
                $"Driver already has a license '{req.LicenseNumber}' issued by '{req.IssuingCountry}'");
        }

        if (req.DocumentId.HasValue)
        {
            var docCheck = await ValidateDocumentOwnershipAsync(req.DocumentId.Value, req.EmployeeId, ct);
            if (!docCheck.IsSuccess)
            {
                return Result<Guid>.Fail(docCheck.Error!);
            }
        }

        var license = DriverLicense.Create(
            req.EmployeeId,
            req.LicenseNumber,
            req.LicenseClass,
            req.IssuingCountry,
            req.IssuedDate,
            req.ExpiresAt,
            req.Endorsements.Combine(),
            req.IssuingRegion,
            req.MedicalCertExpiresAt,
            req.DocumentId);

        await tenantUow.Repository<DriverLicense>().AddAsync(license, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(license.Id);
    }

    private async Task<Result> ValidateDocumentOwnershipAsync(Guid documentId, Guid employeeId, CancellationToken ct)
    {
        var doc = await tenantUow.Repository<EmployeeDocument>().GetByIdAsync(documentId, ct);
        if (doc is null)
        {
            return Result.Fail($"Document '{documentId}' not found");
        }

        if (doc.EmployeeId != employeeId)
        {
            return Result.Fail(
                $"Document '{documentId}' belongs to a different employee and cannot be attached to this license");
        }

        return Result.Ok();
    }
}
