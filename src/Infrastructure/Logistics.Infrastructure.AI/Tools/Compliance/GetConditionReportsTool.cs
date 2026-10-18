using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Compliance.Inspections.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Compliance;

internal sealed class GetConditionReportsTool(IMediator mediator)
    : AgentTool<GetConditionReportsTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The load ID (GUID) whose pickup and delivery condition reports to read")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_condition_reports",
        "Pickup and delivery condition reports for a load, newest first: the vehicle (year, make, model, VIN) or container and seal inspected, who inspected it and when, whether it was signed, and each defect with its part or diagram zone, severity and car hauler BOL damage code (S scratch, D dent, C chip, CR crack, M missing, B broken). Compare a vehicle's pickup and delivery reports to see damage that appeared in transit.")
    {
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetConditionReportsQuery { LoadId = input.LoadId }, ct);
        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Could not read the condition reports");

        var reports = result.Value.Select(r => new
        {
            type = r.Type.ToString(),
            inspected_at = r.InspectedAt.ToString("yyyy-MM-dd HH:mm"),
            inspector = r.InspectorName,
            vehicle = DescribeVehicle(r),
            container_number = r.ContainerNumber,
            seal_number = r.SealNumber,
            signed = r.HasSignature,
            defects = r.Defects.Select(DescribeDefect).ToArray(),
            photo_count = r.Photos.Count,
            notes = r.Notes
        }).ToList();

        return ToolResult.Ok(new { reports, count = reports.Count });
    }

    private static string? DescribeVehicle(ConditionReportDto report)
    {
        var vin = string.IsNullOrEmpty(report.Vin) ? null : $"VIN {report.Vin}";
        var description = string.Join(' ', new[] { report.VehicleYear?.ToString(), report.VehicleMake, report.VehicleModel, vin }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        return description.Length == 0 ? null : description;
    }

    /// <summary>"Front bumper: D, Minor - 3 inch dent" - zone over part, BOL code when the defect has one.</summary>
    private static string DescribeDefect(ConditionDefectDto defect)
    {
        var where = defect.ZoneDisplay ?? defect.PartCategoryDisplay;
        var code = defect.DamageCode is null ? "" : $"{defect.DamageCode}, ";
        var description = string.IsNullOrWhiteSpace(defect.Description) ? "" : $" - {defect.Description}";
        return $"{where}: {code}{defect.SeverityDisplay}{description}";
    }
}
