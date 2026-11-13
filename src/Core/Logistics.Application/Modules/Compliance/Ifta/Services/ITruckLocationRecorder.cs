using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>Records a truck GPS position and adds the distance driven to the truck's per-jurisdiction IFTA totals; the caller saves changes. Every code path that stores a truck position uses it, otherwise IFTA mileage has gaps.</summary>
public interface ITruckLocationRecorder : IApplicationService
{
    Task RecordAsync(
        Truck truck,
        GeoPoint location,
        DateTime timestampUtc,
        EldProviderType? source = null,
        long? odometerMeters = null,
        Address? address = null,
        CancellationToken ct = default);
}
