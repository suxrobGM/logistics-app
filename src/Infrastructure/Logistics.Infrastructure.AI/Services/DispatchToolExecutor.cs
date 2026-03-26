using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MediatR;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchToolExecutor(
    IMediator mediator,
    ITenantUnitOfWork tenantUow) : IDispatchToolExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public async Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default)
    {
        var input = JsonNode.Parse(toolInputJson) ?? new JsonObject();

        return toolName switch
        {
            "get_unassigned_loads" => await GetUnassignedLoadsAsync(ct),
            "get_available_trucks" => await GetAvailableTrucksAsync(ct),
            "get_fleet_overview" => await GetFleetOverviewAsync(ct),
            "get_driver_hos_status" => await GetDriverHosStatusAsync(input, ct),
            "check_hos_feasibility" => await CheckHosFeasibilityAsync(input, ct),
            "calculate_distance" => await CalculateDistanceAsync(input, ct),
            "optimize_trip_stops" => await OptimizeTripStopsAsync(input, ct),
            "search_load_board" => await SearchLoadBoardAsync(input, ct),
            "assign_load_to_truck" => await AssignLoadToTruckAsync(input, ct),
            "create_trip" => await CreateTripAsync(input, ct),
            "dispatch_trip" => await DispatchTripAsync(input, ct),
            "book_load_board_load" => await BookLoadBoardLoadAsync(input, ct),
            _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
        };
    }

    private async Task<string> GetUnassignedLoadsAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery(), ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new
        {
            id = l.Id,
            name = l.Name,
            type = l.Type.ToString(),
            origin = l.OriginAddress?.ToString(),
            destination = l.DestinationAddress?.ToString(),
            origin_lat = l.OriginLocation?.Latitude,
            origin_lng = l.OriginLocation?.Longitude,
            dest_lat = l.DestinationLocation?.Latitude,
            dest_lng = l.DestinationLocation?.Longitude,
            distance_km = l.Distance,
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name
        });

        return JsonSerializer.Serialize(new { loads, count = items.Count }, JsonOptions);
    }

    private async Task<string> GetAvailableTrucksAsync(CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<Truck>()
            .GetListAsync(t => t.Status == TruckStatus.Available, ct);

        var truckData = new List<object>();
        foreach (var truck in trucks)
        {
            DriverHosStatus? hosStatus = null;
            if (truck.MainDriverId is not null)
            {
                hosStatus = await tenantUow.Repository<DriverHosStatus>()
                    .GetAsync(h => h.EmployeeId == truck.MainDriverId.Value, ct);
            }

            truckData.Add(new
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
                    } : null
                } : null
            });
        }

        return JsonSerializer.Serialize(new { trucks = truckData, count = trucks.Count }, JsonOptions);
    }

    private async Task<string> GetFleetOverviewAsync(CancellationToken ct)
    {
        var totalTrucks = await tenantUow.Repository<Truck>().CountAsync(ct: ct);
        var availableTrucks = await tenantUow.Repository<Truck>()
            .CountAsync(t => t.Status == TruckStatus.Available, ct);
        var unassignedLoads = await tenantUow.Repository<Load>()
            .CountAsync(l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0, ct);
        var activeTrips = await tenantUow.Repository<Trip>()
            .CountAsync(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit, ct);
        var hosViolations = await tenantUow.Repository<DriverHosStatus>()
            .CountAsync(h => h.IsInViolation, ct);

        return JsonSerializer.Serialize(new
        {
            total_trucks = totalTrucks,
            available_trucks = availableTrucks,
            unassigned_loads = unassignedLoads,
            active_trips = activeTrips,
            drivers_in_violation = hosViolations
        }, JsonOptions);
    }

    private async Task<string> GetDriverHosStatusAsync(JsonNode input, CancellationToken ct)
    {
        var driverId = Guid.Parse(input["driver_id"]!.GetValue<string>());
        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == driverId, ct);

        if (hos is null)
            return JsonSerializer.Serialize(new { error = "No HOS data found for this driver" });

        return JsonSerializer.Serialize(new
        {
            driver_id = hos.EmployeeId,
            current_duty_status = hos.CurrentDutyStatus.ToString(),
            driving_minutes_remaining = hos.DrivingMinutesRemaining,
            on_duty_minutes_remaining = hos.OnDutyMinutesRemaining,
            cycle_minutes_remaining = hos.CycleMinutesRemaining,
            is_in_violation = hos.IsInViolation,
            is_available = hos.IsAvailableForDispatch(),
            time_until_break = hos.TimeUntilBreakRequired?.ToString(),
            last_updated = hos.LastUpdatedAt
        }, JsonOptions);
    }

    private async Task<string> CheckHosFeasibilityAsync(JsonNode input, CancellationToken ct)
    {
        var driverId = Guid.Parse(input["driver_id"]!.GetValue<string>());
        var distanceKm = input["distance_km"]!.GetValue<double>();

        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == driverId, ct);

        if (hos is null)
            return JsonSerializer.Serialize(new { feasible = false, reason = "No HOS data available for this driver" });

        // Estimate driving time: assume average 80 km/h
        var estimatedDrivingMinutes = (int)(distanceKm / 80.0 * 60);

        var feasible = !hos.IsInViolation
            && hos.DrivingMinutesRemaining >= estimatedDrivingMinutes
            && hos.OnDutyMinutesRemaining >= estimatedDrivingMinutes;

        return JsonSerializer.Serialize(new
        {
            feasible,
            estimated_driving_minutes = estimatedDrivingMinutes,
            driving_minutes_remaining = hos.DrivingMinutesRemaining,
            on_duty_minutes_remaining = hos.OnDutyMinutesRemaining,
            is_in_violation = hos.IsInViolation,
            reason = !feasible
                ? hos.IsInViolation
                    ? "Driver is currently in HOS violation"
                    : $"Insufficient hours: need {estimatedDrivingMinutes}min, have {hos.DrivingMinutesRemaining}min driving"
                : "Driver has sufficient hours for this trip"
        }, JsonOptions);
    }

    private Task<string> CalculateDistanceAsync(JsonNode input, CancellationToken ct)
    {
        var originLat = input["origin_lat"]!.GetValue<double>();
        var originLng = input["origin_lng"]!.GetValue<double>();
        var destLat = input["dest_lat"]!.GetValue<double>();
        var destLng = input["dest_lng"]!.GetValue<double>();

        var origin = new GeoPoint(originLng, originLat);
        var destination = new GeoPoint(destLng, destLat);
        var straightLineKm = origin.DistanceTo(destination) / 1000.0;
        // Estimate driving distance as 1.3x straight-line distance
        var drivingDistanceKm = straightLineKm * 1.3;
        // Estimate duration at average 80 km/h
        var estimatedMinutes = (int)(drivingDistanceKm / 80.0 * 60);

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            straight_line_km = Math.Round(straightLineKm, 1),
            estimated_driving_km = Math.Round(drivingDistanceKm, 1),
            estimated_minutes = estimatedMinutes
        }, JsonOptions));
    }

    private async Task<string> OptimizeTripStopsAsync(JsonNode input, CancellationToken ct)
    {
        // Delegate to ITripOptimizer — simplified for now
        var loadIds = input["load_ids"]!.AsArray()
            .Select(n => Guid.Parse(n!.GetValue<string>())).ToList();

        return JsonSerializer.Serialize(new
        {
            optimized = true,
            load_ids = loadIds,
            message = "Stops will be optimized when the trip is created"
        }, JsonOptions);
    }

    private Task<string> SearchLoadBoardAsync(JsonNode input, CancellationToken ct)
    {
        // Load board search requires tenant-specific provider config
        // Return a placeholder indicating the tool is available
        var originCity = input["origin_city"]?.GetValue<string>();
        var originState = input["origin_state"]?.GetValue<string>();

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            message = $"Load board search for {originCity}, {originState} — requires active load board integration. Check tenant's load board configuration.",
            loads = Array.Empty<object>()
        }, JsonOptions));
    }

    private async Task<string> AssignLoadToTruckAsync(JsonNode input, CancellationToken ct)
    {
        var loadId = Guid.Parse(input["load_id"]!.GetValue<string>());
        var truckId = Guid.Parse(input["truck_id"]!.GetValue<string>());

        var result = await mediator.Send(new AssignLoadToTruckCommand { LoadId = loadId, TruckId = truckId }, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, load_id = loadId, truck_id = truckId })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }

    private async Task<string> CreateTripAsync(JsonNode input, CancellationToken ct)
    {
        var truckId = Guid.Parse(input["truck_id"]!.GetValue<string>());
        var loadIds = input["load_ids"]!.AsArray()
            .Select(n => Guid.Parse(n!.GetValue<string>())).ToList();
        var name = input["name"]?.GetValue<string>() ?? "AI-Generated Trip";

        var command = new CreateTripCommand
        {
            Name = name,
            TruckId = truckId,
            AttachedLoadIds = loadIds
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, trip_id = result.Value })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }

    private async Task<string> DispatchTripAsync(JsonNode input, CancellationToken ct)
    {
        var tripId = Guid.Parse(input["trip_id"]!.GetValue<string>());
        var result = await mediator.Send(new DispatchTripCommand { TripId = tripId }, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, trip_id = tripId })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }

    private Task<string> BookLoadBoardLoadAsync(JsonNode input, CancellationToken ct)
    {
        // Load board booking requires tenant-specific provider config
        return Task.FromResult(JsonSerializer.Serialize(new
        {
            success = false,
            error = "Load board booking requires active load board integration configuration"
        }, JsonOptions));
    }

}
