using System.ComponentModel;
using System.Text.Json;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CheckDispatchEligibilityTool(IDispatchEligibilityService eligibilityService)
    : AgentTool<CheckDispatchEligibilityTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The truck ID (GUID)")]
        public required Guid TruckId { get; init; }

        [Description("The load ID (GUID)")]
        public required Guid LoadId { get; init; }

        [Description("Optional driver ID (GUID). When omitted, the truck's currently assigned main driver is used.")]
        public Guid? DriverId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "check_dispatch_eligibility",
        "Check if a truck (and optionally a specific driver) is eligible to carry a load based on driver license class + endorsements, US Hazmat / EU ADR rules, ADR cert validity, truck Hazmat-placarding, and DOT medical certificate. Returns is_eligible and a list of issues with reason codes (severity: error blocks dispatch, warning is informational). Call this BEFORE dispatch_trip or assign_load_to_truck on hazmat / ADR loads, and whenever the human asks 'can driver X carry load Y'.")
    {
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await eligibilityService.CheckAsync(input.TruckId, input.LoadId, input.DriverId, ct);

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
