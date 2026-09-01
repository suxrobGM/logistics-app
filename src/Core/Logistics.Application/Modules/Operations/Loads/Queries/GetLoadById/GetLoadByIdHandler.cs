using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Identity.Roles;
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
        var notFound = Result<LoadDto>.Fail($"Could not find a load with ID '{req.Id}'");

        if (load is null)
        {
            return notFound;
        }

        var isDriver = currentUserService.IsInRole(TenantRoles.Driver) &&
                       !currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin);
        if (isDriver && !DrivenByCaller(load))
        {
            return notFound;
        }

        return Result<LoadDto>.Ok(load.ToDto(LoadIntermodalLookup.Empty));
    }

    private bool DrivenByCaller(Load load)
    {
        var callerId = currentUserService.GetUserId();
        return callerId is not null &&
               load.AssignedTruck is { } truck &&
               (truck.MainDriverId == callerId || truck.SecondaryDriverId == callerId);
    }
}
