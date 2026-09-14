using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Common.Services;

/// <summary>
/// Gates car-hauling load and truck types on <see cref="TenantFeature.VehicleTransport"/>. The type is a
/// field value, so <c>[RequiresFeature]</c> cannot do it - every write path that accepts one calls this.
/// </summary>
public interface IVehicleTransportGuard : IApplicationService
{
    Task<Result> CheckLoadTypeAsync(LoadType? type);
    Task<Result> CheckLoadTypesAsync(IEnumerable<LoadType>? types);
    Task<Result> CheckTruckTypeAsync(TruckType? type);
}
