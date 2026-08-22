using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class BatchCheckHosFeasibilityTool(ITenantUnitOfWork tenantUow)
    : AgentTool<BatchCheckHosFeasibilityTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Array of driver/distance pairs to check")]
        public required Check[] Checks { get; init; }
    }

    internal sealed record Check
    {
        [Description("The driver's employee ID (GUID)")]
        public required Guid DriverId { get; init; }

        [Description("Estimated driving distance in kilometers")]
        public required double DistanceKm { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "batch_check_hos_feasibility",
        "Check HOS feasibility for multiple driver-distance pairs in a single call. More efficient than calling check_hos_feasibility multiple times. Returns feasibility result for each pair.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (input.Checks.Length == 0)
            return ToolResult.Error("No valid checks provided");

        var driverIds = input.Checks.Select(c => c.DriverId).Distinct().ToList();
        var hosStatuses = (await tenantUow.Repository<DriverHosStatus>()
            .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
            .ToDictionary(h => h.EmployeeId);

        var results = input.Checks.Select(check =>
        {
            var hos = hosStatuses.GetValueOrDefault(check.DriverId);
            var verdict = hos is null
                ? HosFeasibility.Unknown(check.DistanceKm)
                : HosFeasibility.Evaluate(hos, check.DistanceKm);

            return new AgentToolHosCheckDto
            {
                DriverId = check.DriverId,
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
