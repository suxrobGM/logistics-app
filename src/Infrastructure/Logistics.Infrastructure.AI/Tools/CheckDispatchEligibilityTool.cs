using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Dispatch;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CheckDispatchEligibilityTool(IDispatchEligibilityService eligibilityService)
    : IAiDispatchTool
{
    public string Name => "check_dispatch_eligibility";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["truck_id"]?.GetValue<string>(), out var truckId))
        {
            return JsonSerializer.Serialize(new { error = "Invalid or missing truck_id" });
        }

        if (!Guid.TryParse(input["load_id"]?.GetValue<string>(), out var loadId))
        {
            return JsonSerializer.Serialize(new { error = "Invalid or missing load_id" });
        }

        Guid? driverId = null;
        var driverIdRaw = input["driver_id"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(driverIdRaw))
        {
            if (!Guid.TryParse(driverIdRaw, out var parsedDriverId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid driver_id" });
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
