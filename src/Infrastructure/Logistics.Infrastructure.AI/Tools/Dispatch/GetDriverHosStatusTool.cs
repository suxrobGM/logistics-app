using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetDriverHosStatusTool(ITenantUnitOfWork tenantUow)
    : AgentTool<GetDriverHosStatusTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The driver's employee ID (GUID)")]
        public required Guid DriverId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_driver_hos_status",
        "Get detailed HOS (Hours of Service) status for a specific driver. Returns current duty status, driving minutes remaining, on-duty minutes remaining, cycle minutes remaining, violation status, and next mandatory break time.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var hos = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == input.DriverId, ct);

        if (hos is null)
            return ToolResult.Error("No HOS data found for this driver");

        return ToolResult.Ok(new
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
        });
    }
}
