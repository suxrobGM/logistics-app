using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetLoadByIdQuery, Load, LoadDto>(tenantUow)
{
    // Single row, so there is no N+1 to batch away - the nav properties lazy-load at most twice.
    protected override LoadDto MapToDto(Load entity) => entity.ToDto(LoadIntermodalLookup.Empty);
}
