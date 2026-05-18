using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

internal sealed class GetDriverLicensesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDriverLicensesQuery, Result<IList<DriverLicenseDto>>>
{
    public async Task<Result<IList<DriverLicenseDto>>> Handle(
        GetDriverLicensesQuery req, CancellationToken ct)
    {
        var licenses = await tenantUow.Repository<DriverLicense>()
            .GetListAsync(i => i.EmployeeId == req.EmployeeId, ct);

        if (!req.IncludeRevoked)
        {
            licenses = licenses.Where(i => i.Status != DriverLicenseStatus.Revoked).ToList();
        }

        var dtos = licenses
            .OrderByDescending(i => i.ExpiresAt)
            .Select(i => i.ToDto())
            .ToList();

        return Result<IList<DriverLicenseDto>>.Ok(dtos);
    }
}
