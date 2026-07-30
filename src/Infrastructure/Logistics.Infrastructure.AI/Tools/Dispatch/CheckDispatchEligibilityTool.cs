using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Dispatch;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CheckDispatchEligibilityTool(IDispatchEligibilityService eligibilityService)
    : IAIDispatchTool
{
    public string Name => "check_dispatch_eligibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("truck_id") is not { } truckId)
        {
            return ToolResult.Error("Invalid or missing truck_id");
        }

        if (input.GetGuid("load_id") is not { } loadId)
        {
            return ToolResult.Error("Invalid or missing load_id");
        }

        Guid? driverId = null;
        var driverIdRaw = input.GetString("driver_id");
        if (!string.IsNullOrEmpty(driverIdRaw))
        {
            if (!Guid.TryParse(driverIdRaw, out var parsedDriverId))
            {
                return ToolResult.Error("Invalid driver_id");
            }
            driverId = parsedDriverId;
        }

        var result = await eligibilityService.CheckAsync(truckId, loadId, driverId, ct);

        return JsonSerializer.Serialize(new
        {
            is_eligible = result.IsEligible,
            issues = result.Issues
                .Select(i => new
                {
                    code = i.Code.ToString(),
                    severity = i.Severity.ToString().ToLowerInvariant(),
                    message = i.Message
                })
                .ToArray()
        });
    }
}
