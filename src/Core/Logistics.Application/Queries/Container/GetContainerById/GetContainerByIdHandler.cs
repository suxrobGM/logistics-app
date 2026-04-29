using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetContainerByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetContainerByIdQuery, Result<ContainerDto>>
{
    public async Task<Result<ContainerDto>> Handle(GetContainerByIdQuery req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);

        if (container is null)
        {
            return Result<ContainerDto>.Fail($"Could not find container with ID '{req.Id}'");
        }

        return Result<ContainerDto>.Ok(container.ToDto());
    }
}
