using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadByIdHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    IVehicleCapacityChecker vehicleCapacityChecker)
    : IRequestHandler<GetLoadByIdQuery, Result<LoadDto>>
{
    public async Task<Result<LoadDto>> Handle(GetLoadByIdQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null || !currentUserService.CanDrive(load.AssignedTruck))
        {
            return Result<LoadDto>.Fail($"Could not find a load with ID '{req.Id}'");
        }

        var dto = load.ToDto(LoadIntermodalLookup.Empty);
        dto.Accessorials = load.Accessorials.Select(a => a.ToDto()).ToList();
        var trip = load.TripStops.FirstOrDefault()?.Trip;
        dto.TrailerId = trip?.TrailerId;
        dto.TrailerNumber = trip?.Trailer?.Number;

        if (load is { Type: LoadType.Vehicle, AssignedTruck: not null })
        {
            dto.VehicleCapacityWarning = await vehicleCapacityChecker.GetWarningAsync(load.AssignedTruck, ct: ct);
        }

        return Result<LoadDto>.Ok(dto);
    }
}
