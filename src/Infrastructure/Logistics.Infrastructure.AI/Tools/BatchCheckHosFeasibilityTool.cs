using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class BatchCheckHosFeasibilityTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "batch_check_hos_feasibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var checksNode = input["checks"]?.AsArray();
        if (checksNode is null || checksNode.Count == 0)
            return JsonSerializer.Serialize(new { error = "Missing or empty 'checks' array" });

        // Parse all check requests
        var checks = checksNode
            .Where(c => c is not null)
            .Select(c => new
            {
                DriverId = Guid.TryParse(c!["driver_id"]?.GetValue<string>(), out var id) ? id : (Guid?)null,
                DistanceKm = c["distance_km"]?.GetValue<double>() ?? 0
            })
            .Where(c => c.DriverId is not null)
            .ToList();

        if (checks.Count == 0)
            return JsonSerializer.Serialize(new { error = "No valid checks provided" });

        // Batch-load all HOS statuses in one query
        var driverIds = checks.Select(c => c.DriverId!.Value).Distinct().ToList();
        var hosStatuses = (await tenantUow.Repository<DriverHosStatus>()
            .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
            .ToDictionary(h => h.EmployeeId);

        var results = checks.Select(check =>
        {
            var driverId = check.DriverId!.Value;
            var estimatedDrivingMinutes = (int)(check.DistanceKm / 80.0 * 60);

            if (!hosStatuses.TryGetValue(driverId, out var hos))
                return new
                {
                    driver_id = driverId.ToString(),
                    distance_km = check.DistanceKm,
                    feasible = false,
                    feasible_multi_day = false,
                    estimated_driving_minutes = estimatedDrivingMinutes,
                    driving_minutes_remaining = (int?)null,
                    on_duty_minutes_remaining = (int?)null,
                    reason = "No HOS data available for this driver"
                };

            var singleWindowFeasible = !hos.IsInViolation
                && hos.DrivingMinutesRemaining >= estimatedDrivingMinutes
                && hos.OnDutyMinutesRemaining >= estimatedDrivingMinutes;

            var multiDay = !singleWindowFeasible && !hos.IsInViolation
                && hos.DrivingMinutesRemaining >= 120;

            var reason = $"Insufficient hours: need {estimatedDrivingMinutes}min, have {hos.DrivingMinutesRemaining}min driving - too low to make meaningful progress";
            if (singleWindowFeasible)
            {
                reason = "Driver has sufficient hours to complete in one stretch";
            }
            else if (hos.IsInViolation)
            {
                reason = "Driver is currently in HOS violation";
            }
            else if (multiDay)
            {
                reason = $"Not completable in current window (need {estimatedDrivingMinutes}min, have {hos.DrivingMinutesRemaining}min), but feasible as multi-day trip with rest stops";
            }

            return new
            {
                driver_id = driverId.ToString(),
                distance_km = check.DistanceKm,
                feasible = singleWindowFeasible,
                feasible_multi_day = multiDay,
                estimated_driving_minutes = estimatedDrivingMinutes,
                driving_minutes_remaining = (int?)hos.DrivingMinutesRemaining,
                on_duty_minutes_remaining = (int?)hos.OnDutyMinutesRemaining,
                reason
            };
        }).ToList();

        return JsonSerializer.Serialize(new { results, count = results.Count });
    }
}
