using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal sealed class VehicleTransportFeatureChecker(ITenantUnitOfWork tenantUow, IFeatureService featureService)
    : IVehicleTransportFeatureChecker
{
    public Task<Result> CheckLoadTypeAsync(LoadType? type) =>
        type is LoadType.Vehicle ? CheckFeatureAsync() : Task.FromResult(Result.Ok());

    public Task<Result> CheckLoadTypesAsync(IEnumerable<LoadType>? types) =>
        types?.Contains(LoadType.Vehicle) == true ? CheckFeatureAsync() : Task.FromResult(Result.Ok());

    public Task<Result> CheckTruckTypeAsync(TruckType? type) =>
        type is { } t && TruckLoadCompatibility.VehicleTruckTypes.Contains(t) ? CheckFeatureAsync() : Task.FromResult(Result.Ok());

    private Task<Result> CheckFeatureAsync() =>
        featureService.CheckFeatureAsync(tenantUow.GetCurrentTenant().Id, TenantFeature.VehicleTransport);
}
