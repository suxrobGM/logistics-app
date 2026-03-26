using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetAvailableTrucksTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "get_available_trucks";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<Truck>()
            .GetListAsync(t => t.Status == TruckStatus.Available, ct);

        // Batch-load HOS statuses to avoid N+1 queries
        var driverIds = trucks
            .Where(t => t.MainDriverId is not null)
            .Select(t => t.MainDriverId!.Value)
            .ToList();

        var hosStatuses = driverIds.Count > 0
            ? (await tenantUow.Repository<DriverHosStatus>()
                .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
                .ToDictionary(h => h.EmployeeId)
            : [];

        var truckData = trucks.Select(truck =>
        {
            var hosStatus = truck.MainDriverId is not null
                && hosStatuses.TryGetValue(truck.MainDriverId.Value, out var hos) ? hos : null;

            return new
            {
                id = truck.Id,
                number = truck.Number,
                type = truck.Type.ToString(),
                current_lat = truck.CurrentLocation?.Latitude,
                current_lng = truck.CurrentLocation?.Longitude,
                current_address = truck.CurrentAddress?.ToString(),
                main_driver = truck.MainDriver is not null ? new
                {
                    id = truck.MainDriver.Id,
                    name = truck.MainDriver.GetFullName(),
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

        return JsonSerializer.Serialize(new { trucks = truckData, count = trucks.Count });
    }
}
