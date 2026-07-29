using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CheckHosFeasibilityTool(ITenantUnitOfWork tenantUow) : IAIDispatchTool
{
    public string Name => "check_hos_feasibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["driver_id"]?.GetValue<string>(), out var driverId))
            return ToolResult.Error("Invalid or missing driver_id");

        var distanceKm = input["distance_km"]?.GetValue<double>() ?? 0;

        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == driverId, ct);

        // Deliberately a shorter payload than the batch tool's no-data arm.
        if (hos is null)
            return ToolResult.Ok(new
            {
                feasible = false,
                reason = HosFeasibility.Unknown(distanceKm).Reason
            });

        var verdict = HosFeasibility.Evaluate(hos, distanceKm);

        return ToolResult.Ok(new
        {
            feasible = verdict.Feasible,
            feasible_multi_day = verdict.FeasibleMultiDay,
            estimated_driving_minutes = verdict.EstimatedDrivingMinutes,
            driving_minutes_remaining = hos.DrivingMinutesRemaining,
            on_duty_minutes_remaining = hos.OnDutyMinutesRemaining,
            is_in_violation = hos.IsInViolation,
            reason = verdict.Reason
        });
    }
}
