using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetAvailableTrucksTool(ITenantUnitOfWork tenantUow)
    : AgentTool<NoToolInput>, IAgentToolMetadata
{
    public static AgentToolDefinition Definition => new(
        "get_available_trucks",
        "Get all trucks with Available status along with their driver info, HOS (Hours of Service) status, and a fleet summary (total trucks, available trucks, active trips, drivers in violation). Returns truck ID, number, type, vehicle_capacity for car haulers, current_trailer (number and type, e.g. Reefer or Chassis; null when none is hooked), current location, driver name, and remaining driving/on-duty hours.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(NoToolInput input, CancellationToken ct)
    {
        var fleet = await AvailableFleet.LoadAsync(tenantUow, ct);

        var totalTrucks = await tenantUow.Repository<Truck>().CountAsync(ct: ct);
        var activeTrips = await tenantUow.Repository<Trip>()
            .CountAsync(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit, ct);
        var driversInViolation = fleet.Count(f => f.Hos?.IsInViolation == true);

        var truckData = fleet.Select(entry =>
        {
            var (truck, driver, hosStatus, trailer) = entry;

            return new AgentToolTruckDto
            {
                Id = truck.Id,
                Number = truck.Number,
                Type = truck.Type.ToString(),
                CurrentLat = truck.CurrentLocation?.Latitude,
                CurrentLng = truck.CurrentLocation?.Longitude,
                CurrentAddress = truck.CurrentAddress?.ToString(),
                VehicleCapacity = truck.VehicleCapacity > 0 ? truck.VehicleCapacity : null,
                CurrentTrailer = trailer is null ? null : new AgentToolTrailerDto
                {
                    Number = trailer.Number,
                    Type = trailer.Type.ToString()
                },
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
            Count = fleet.Count,
            FleetSummary = new AgentToolFleetSummaryDto
            {
                TotalTrucks = totalTrucks,
                AvailableTrucks = fleet.Count,
                ActiveTrips = activeTrips,
                DriversInViolation = driversInViolation
            }
        });
    }
}
