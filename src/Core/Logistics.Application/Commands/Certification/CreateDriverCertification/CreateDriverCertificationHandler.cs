using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateDriverCertificationHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateDriverCertificationCommand, Result<DriverCertificationDto>>
{
    public async Task<Result<DriverCertificationDto>> Handle(CreateDriverCertificationCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);
        if (employee is null)
        {
            return Result<DriverCertificationDto>.Fail("Employee not found.");
        }

        var status = DetermineCertificationStatus(req.ExpirationDate);

        var certification = new DriverCertification
        {
            EmployeeId = req.EmployeeId,
            Employee = employee,
            CertificationType = req.Type,
            Status = status,
            CertificationNumber = req.CertificationNumber,
            IssuingAuthority = req.IssuingAuthority,
            IssuingState = req.IssuingState,
            IssuedDate = req.IssueDate,
            ExpirationDate = req.ExpirationDate,
            CdlClass = req.CdlClass,
            Endorsements = req.Endorsements,
            Restrictions = req.Restrictions,
            Notes = req.Notes,
            IsVerified = false
        };

        await tenantUow.Repository<DriverCertification>().AddAsync(certification, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<DriverCertificationDto>.Ok(certification.ToDto());
    }

    private static CertificationStatus DetermineCertificationStatus(DateTime expirationDate)
    {
        var daysUntilExpiration = (expirationDate - DateTime.UtcNow).Days;

        if (daysUntilExpiration < 0)
            return CertificationStatus.Expired;
        if (daysUntilExpiration <= 30)
            return CertificationStatus.ExpiringSoon;

        return CertificationStatus.Active;
    }
}
