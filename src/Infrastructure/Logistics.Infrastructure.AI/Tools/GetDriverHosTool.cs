using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetDriverHosTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "get_driver_hos_status";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
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
        });
    }
}
