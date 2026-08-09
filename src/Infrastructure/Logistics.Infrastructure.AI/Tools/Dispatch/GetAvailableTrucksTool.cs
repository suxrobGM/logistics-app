using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetAvailableTrucksTool(ITenantUnitOfWork tenantUow) : IAgentTool
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

            return new AgentToolTruckDto
            {
                Id = truck.Id,
                Number = truck.Number,
                Type = truck.Type.ToString(),
                CurrentLat = truck.CurrentLocation?.Latitude,
                CurrentLng = truck.CurrentLocation?.Longitude,
                CurrentAddress = truck.CurrentAddress?.ToString(),
                MainDriver = driver is null ? null : new AgentToolDriverDto
                {
                    Id = driver.Id,
                    Name = driver.GetFullName(),
                    Hos = hosStatus is null ? null : new AgentToolHosDto
                    {
                        DrivingMinutesRemaining = hosStatus.DrivingMinutesRemaining,
                        OnDutyMinutesRemaining = hosStatus.OnDutyMinutesRemaining,
                        CycleMinutesRemaining = hosStatus.CycleMinutesRemaining,
                        IsInViolation = hosStatus.IsInViolation,
                        IsAvailable = hosStatus.IsAvailableForDispatch()
                    }
                }
            };
        }).ToList();

        return ToolResult.Typed(new AgentToolResultDto
        {
            Trucks = truckData,
            Count = trucks.Count,
            FleetSummary = new AgentToolFleetSummaryDto
            {
                TotalTrucks = totalTrucks,
                AvailableTrucks = trucks.Count,
                ActiveTrips = activeTrips,
                DriversInViolation = driversInViolation
            }
        });
    }
}
