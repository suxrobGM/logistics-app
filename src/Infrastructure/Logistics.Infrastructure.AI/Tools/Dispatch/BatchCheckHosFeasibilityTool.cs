using Logistics.Application.Abstractions.Agents;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class BatchCheckHosFeasibilityTool(ITenantUnitOfWork tenantUow) : IAgentTool
{
    public string Name => "batch_check_hos_feasibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var checksNode = input["checks"]?.AsArray();
        if (checksNode is null || checksNode.Count == 0)
            return ToolResult.Error("Missing or empty 'checks' array");

        var checks = checksNode
            .Where(c => c is not null)
            .Select(c => new
            {
                DriverId = c!.GetGuid("driver_id"),
                DistanceKm = c.GetDouble("distance_km") ?? 0
            })
            .Where(c => c.DriverId is not null)
            .ToList();

        if (checks.Count == 0)
            return ToolResult.Error("No valid checks provided");

        var driverIds = checks.Select(c => c.DriverId!.Value).Distinct().ToList();
        var hosStatuses = (await tenantUow.Repository<DriverHosStatus>()
            .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
            .ToDictionary(h => h.EmployeeId);

        var results = checks.Select(check =>
        {
            var driverId = check.DriverId!.Value;
            var hos = hosStatuses.GetValueOrDefault(driverId);
            var verdict = hos is null
                ? HosFeasibility.Unknown(check.DistanceKm)
                : HosFeasibility.Evaluate(hos, check.DistanceKm);

            return new AgentToolHosCheckDto
            {
                DriverId = driverId,
                DistanceKm = check.DistanceKm,
                Feasible = verdict.Feasible,
                FeasibleMultiDay = verdict.FeasibleMultiDay,
                EstimatedDrivingMinutes = verdict.EstimatedDrivingMinutes,
                DrivingMinutesRemaining = hos?.DrivingMinutesRemaining,
                OnDutyMinutesRemaining = hos?.OnDutyMinutesRemaining,
                Reason = verdict.Reason
            };
        }).ToList();

        return ToolResult.Typed(new AgentToolResultDto { Results = results, Count = results.Count });
    }
}
