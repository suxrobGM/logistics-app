using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

internal sealed class GetDvirReportByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetDvirReportByIdQuery, DvirReport, DvirReportDto>(tenantUow)
{
    protected override DvirReportDto MapToDto(DvirReport entity) => entity.ToDto();
}
