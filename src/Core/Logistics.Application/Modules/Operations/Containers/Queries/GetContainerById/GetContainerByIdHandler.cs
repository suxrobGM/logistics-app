using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

internal sealed class GetContainerByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetContainerByIdQuery, Container, ContainerDto>(tenantUow)
{
    protected override ContainerDto MapToDto(Container entity) => entity.ToDto();
}
