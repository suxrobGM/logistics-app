using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CheckHosFeasibilityTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "check_hos_feasibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
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
        });
    }
}
