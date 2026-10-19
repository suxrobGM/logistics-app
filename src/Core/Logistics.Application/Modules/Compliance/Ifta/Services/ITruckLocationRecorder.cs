using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>
/// Records a truck GPS ping: appends a <see cref="TruckLocationHistory"/> breadcrumb,
/// accrues guarded segment distance into the per-jurisdiction daily mileage rollup, and
/// updates <see cref="Truck.CurrentLocation"/>. A ping that moves the truck replaces
/// <see cref="Truck.CurrentAddress"/> with its address, clearing it when the ping has none (ELD), so
/// the address never names a place the truck has left. All GPS write paths (ELD sync, driver app)
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
        Address? address = null,
        CancellationToken ct = default);
}
