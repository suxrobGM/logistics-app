using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Common.Services;

internal sealed class VehicleTransportGuard(ITenantUnitOfWork tenantUow, IFeatureService featureService)
    : IVehicleTransportGuard
{
    public Task<Result> CheckLoadTypeAsync(LoadType? type) =>
        type is LoadType.Vehicle ? CheckFeatureAsync() : Task.FromResult(Result.Ok());

    public Task<Result> CheckTruckTypeAsync(TruckType? type) =>
        type is TruckType.CarHauler or TruckType.CarTransporter ? CheckFeatureAsync() : Task.FromResult(Result.Ok());

    private async Task<Result> CheckFeatureAsync()
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;

        return await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.VehicleTransport)
            ? Result.Ok()
            : Result.Fail(
                "Vehicle transport is not enabled for your company. Contact support to enable it.",
                ErrorCodes.FeatureDisabledByAdmin);
    }
}
