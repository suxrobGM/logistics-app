using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Intermodal;

/// <summary>Agent tool returning terminal details. Terminals have no coordinates, so its output cannot be passed to <c>calculate_distance</c>.</summary>
internal sealed class GetTerminalInfoTool(ITenantUnitOfWork tenantUow)
    : AgentTool<GetTerminalInfoTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("UN/LOCODE, e.g. 'USLAX' (Los Angeles), 'BEANR' (Antwerp)")]
        public string? Code { get; init; }

        [Description("Terminal ID (GUID) - use only when the code is unknown")]
        public Guid? TerminalId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_terminal_info",
        "Look up an intermodal terminal by UN/LOCODE. Returns name, type (SeaPort, RailTerminal, InlandDepot, AirCargo, BorderCrossing), country, street address, and how many containers are currently sitting there. Terminals carry no coordinates - keep using the load's origin_lat/origin_lng for deadhead math.")
    {
        RequiredFeature = TenantFeature.IntermodalContainers,
        RequiredPermission = Permission.AIDispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var code = input.Code?.Trim();
        Terminal? terminal;

        if (!string.IsNullOrEmpty(code))
        {
            // Stored codes are canonical, so normalise the term and compare exactly - that lookup
            // the unique index can serve.
            var normalized = Terminal.NormalizeCode(code);
            terminal = await tenantUow.Repository<Terminal>()
                .GetAsync(t => t.Code == normalized, ct);
        }
        else if (input.TerminalId is { } terminalId)
        {
            terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(terminalId, ct);
        }
        else
        {
            return ToolResult.Error("Provide code (UN/LOCODE) or terminal_id");
        }

        if (terminal is null)
            return ToolResult.Error($"No terminal found for {code ?? input.TerminalId?.ToString()}");

        return ToolResult.Ok(new
        {
            id = terminal.Id,
            name = terminal.Name,
            code = terminal.Code,
            type = terminal.Type.ToString(),
            type_description = terminal.Type.GetDescription(),
            country_code = terminal.Address.Country,
            address = terminal.Address.ToString(),
            city = terminal.Address.City,
            notes = terminal.Notes
        });
    }
}
