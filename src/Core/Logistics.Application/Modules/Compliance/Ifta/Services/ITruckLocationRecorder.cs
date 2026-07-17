using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>
/// Records a truck GPS ping: appends a <see cref="TruckLocationHistory"/> breadcrumb,
/// accrues guarded segment distance into the per-jurisdiction daily mileage rollup, and
/// updates <see cref="Truck.CurrentLocation"/>. All GPS write paths (ELD sync, driver app)
/// must go through this so IFTA mileage stays complete. The caller owns SaveChangesAsync.
/// </summary>
public interface ITruckLocationRecorder : IApplicationService
{
    Task RecordAsync(
        Truck truck,
        GeoPoint location,
        DateTime timestampUtc,
        EldProviderType? source = null,
        int? odometerReading = null,
        CancellationToken ct = default);
}
