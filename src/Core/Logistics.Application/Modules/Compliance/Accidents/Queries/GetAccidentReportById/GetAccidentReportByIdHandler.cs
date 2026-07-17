using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Accidents.Queries;

internal sealed class GetAccidentReportByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetAccidentReportByIdQuery, AccidentReport, AccidentReportDto>(tenantUow)
{
    protected override AccidentReportDto MapToDto(AccidentReport entity) => entity.ToDto();
}
