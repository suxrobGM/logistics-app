using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetLoadByIdQuery, Load, LoadDto>(tenantUow)
{
    protected override LoadDto MapToDto(Load entity) => entity.ToDto();
}
