using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

internal sealed class GetTimeEntryByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetTimeEntryByIdQuery, TimeEntry, TimeEntryDto>(tenantUow)
{
    protected override TimeEntryDto MapToDto(TimeEntry entity) => entity.ToDto();
}
