using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CheckHosFeasibilityTool(ITenantUnitOfWork tenantUow)
    : AgentTool<CheckHosFeasibilityTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The driver's employee ID (GUID)")]
        public required Guid DriverId { get; init; }

        [Description("Estimated driving distance, in distance_unit")]
        public required double Distance { get; init; }

        [Description("The unit of distance - the suffix of the key it was read from: _km is Kilometers, _mi is Miles")]
        public required DistanceUnit DistanceUnit { get; init; }

        internal double Km() => DispatchUnits.MetersToKm(DistanceUnit.ToMeters(Distance));
    }

    public static AgentToolDefinition Definition => new(
        "check_hos_feasibility",
        "Check if a driver can feasibly complete a trip given the estimated driving distance. Returns whether the driver has enough HOS hours remaining and details about any constraints.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == input.DriverId, ct);

        // Deliberately a shorter payload than the batch tool's no-data arm.
        if (hos is null)
            return ToolResult.Typed(new AgentToolResultDto
            {
                Feasible = false,
                Reason = HosFeasibility.Unknown(input.Km()).Reason
            });

        var verdict = HosFeasibility.Evaluate(hos, input.Km());

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
