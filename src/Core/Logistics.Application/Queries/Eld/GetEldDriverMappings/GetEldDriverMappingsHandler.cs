using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetEldDriverMappingsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetEldDriverMappingsQuery, Result<List<EldDriverMappingDto>>>
{
    public async Task<Result<List<EldDriverMappingDto>>> Handle(GetEldDriverMappingsQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetByIdAsync(req.ProviderId, ct);

        if (config is null)
        {
            return Result<List<EldDriverMappingDto>>.Fail("ELD provider configuration not found");
        }

        var mappings = await tenantUow.Repository<EldDriverMapping>()
            .Query()
            .Include(m => m.Employee)
            .Where(m => m.ProviderType == config.ProviderType)
            .Select(m => new EldDriverMappingDto
            {
                Id = m.Id,
                EmployeeId = m.EmployeeId,
                EmployeeName = m.Employee.FirstName + " " + m.Employee.LastName,
                ProviderType = m.ProviderType,
                ExternalDriverId = m.ExternalDriverId,
                ExternalDriverName = m.ExternalDriverName,
                IsSyncEnabled = m.IsSyncEnabled,
                LastSyncedAt = m.LastSyncedAt
            })
            .ToListAsync(ct);

        return Result<List<EldDriverMappingDto>>.Ok(mappings);
    }
}
