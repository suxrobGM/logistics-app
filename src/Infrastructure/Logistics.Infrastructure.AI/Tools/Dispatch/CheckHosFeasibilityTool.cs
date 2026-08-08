using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CheckHosFeasibilityTool(ITenantUnitOfWork tenantUow) : IAgentTool
{
    public string Name => "check_hos_feasibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("driver_id") is not { } driverId)
            return ToolResult.Error("Invalid or missing driver_id");

        var distanceKm = input.GetDouble("distance_km") ?? 0;

        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == driverId, ct);

        // Deliberately a shorter payload than the batch tool's no-data arm.
        if (hos is null)
            return ToolResult.Typed(new AgentToolResultDto
            {
                Feasible = false,
                Reason = HosFeasibility.Unknown(distanceKm).Reason
            });

        var verdict = HosFeasibility.Evaluate(hos, distanceKm);

        return ToolResult.Typed(new AgentToolResultDto
        {
            Feasible = verdict.Feasible,
            FeasibleMultiDay = verdict.FeasibleMultiDay,
            EstimatedDrivingMinutes = verdict.EstimatedDrivingMinutes,
            DrivingMinutesRemaining = hos.DrivingMinutesRemaining,
            OnDutyMinutesRemaining = hos.OnDutyMinutesRemaining,
            IsInViolation = hos.IsInViolation,
            Reason = verdict.Reason
        });
    }
}
