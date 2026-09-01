using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadByIdHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<GetLoadByIdQuery, Result<LoadDto>>
{
    public async Task<Result<LoadDto>> Handle(GetLoadByIdQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null || !CanRead(load))
        {
            return Result<LoadDto>.Fail($"Could not find a load with ID '{req.Id}'");
        }

        return Result<LoadDto>.Ok(load.ToDto(LoadIntermodalLookup.Empty));
    }

    private bool CanRead(Load load)
    {
        return !currentUserService.IsTenantDriver() ||
               (currentUserService.GetUserId() is { } callerId && load.IsDrivenBy(callerId));
    }
}
