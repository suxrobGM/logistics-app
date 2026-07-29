using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetAvailableTrucksTool(ITenantUnitOfWork tenantUow) : IAIDispatchTool
{
    public string Name => "get_available_trucks";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<Truck>()
            .GetListAsync(t => t.Status == TruckStatus.Available, ct);

        var driverIds = trucks
            .Where(t => t.MainDriverId is not null)
            .Select(t => t.MainDriverId!.Value)
            .ToList();

        // Both the HOS rows and the drivers themselves are batched. Truck.MainDriver is a lazy
        // navigation, so reading it inside the projection below would be one SELECT per truck - in
        // the tool the system prompt tells the agent to call first on every run.
        var hosStatuses = driverIds.Count > 0
            ? (await tenantUow.Repository<DriverHosStatus>()
                .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
                .ToDictionary(h => h.EmployeeId)
            : [];

        var drivers = driverIds.Count > 0
            ? (await tenantUow.Repository<Employee>()
                .GetListAsync(e => driverIds.Contains(e.Id), ct))
                .ToDictionary(e => e.Id)
            : [];

        var totalTrucks = await tenantUow.Repository<Truck>().CountAsync(ct: ct);
        var activeTrips = await tenantUow.Repository<Trip>()
            .CountAsync(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit, ct);
        var driversInViolation = hosStatuses.Values.Count(h => h.IsInViolation);

        var truckData = trucks.Select(truck =>
        {
            var hosStatus = truck.MainDriverId is not null
                && hosStatuses.TryGetValue(truck.MainDriverId.Value, out var hos) ? hos : null;
            var driver = truck.MainDriverId is not null
                && drivers.TryGetValue(truck.MainDriverId.Value, out var found) ? found : null;

            return new
            {
                id = truck.Id,
                number = truck.Number,
                type = truck.Type.ToString(),
                current_lat = truck.CurrentLocation?.Latitude,
                current_lng = truck.CurrentLocation?.Longitude,
                current_address = truck.CurrentAddress?.ToString(),
                main_driver = driver is not null ? new
                {
                    id = driver.Id,
                    name = driver.GetFullName(),
                    hos = hosStatus is not null ? new
                    {
                        driving_minutes_remaining = hosStatus.DrivingMinutesRemaining,
                        on_duty_minutes_remaining = hosStatus.OnDutyMinutesRemaining,
                        cycle_minutes_remaining = hosStatus.CycleMinutesRemaining,
                        is_in_violation = hosStatus.IsInViolation,
                        is_available = hosStatus.IsAvailableForDispatch()
                    } : (object?)null
                } : (object?)null
            };
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            trucks = truckData,
            count = trucks.Count,
            fleet_summary = new
            {
                total_trucks = totalTrucks,
                available_trucks = trucks.Count,
                active_trips = activeTrips,
                drivers_in_violation = driversInViolation
            }
        });
    }
}
