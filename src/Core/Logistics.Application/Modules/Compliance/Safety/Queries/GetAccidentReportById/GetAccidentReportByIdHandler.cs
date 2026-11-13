using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Queries;

internal sealed class GetAccidentReportByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetAccidentReportByIdQuery, AccidentReport, AccidentReportDto>(tenantUow)
{
    protected override AccidentReportDto MapToDto(AccidentReport entity) => entity.ToDto();
}
