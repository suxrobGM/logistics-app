using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Looks up an intermodal terminal by UN/LOCODE (or id) for pickup / drop-off context.
/// Terminals carry no coordinates, so this cannot feed <c>calculate_distance</c> - the deadhead
/// anchor stays the load's origin latitude / longitude.
/// </summary>
internal sealed class GetTerminalInfoTool(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService) : IAiDispatchTool
{
    public string Name => "get_terminal_info";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        // The agent never gets this tool ungated, but MCP lists every tool - so the gate holds here.
        var tenant = tenantUow.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.IntermodalContainers))
        {
            return JsonSerializer.Serialize(new
            {
                error = "Intermodal container tracking is not enabled for this tenant"
            });
        }

        var code = input["code"]?.GetValue<string>()?.Trim();
        var idRaw = input["terminal_id"]?.GetValue<string>();

        Terminal? terminal;

        if (!string.IsNullOrEmpty(code))
        {
            // Stored codes are canonical, so normalise the term and compare exactly - that lookup
            // the unique index can serve.
            var normalized = Terminal.NormalizeCode(code);
            terminal = await tenantUow.Repository<Terminal>()
                .GetAsync(t => t.Code == normalized, ct);
        }
        else if (Guid.TryParse(idRaw, out var terminalId))
        {
            terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(terminalId, ct);
        }
        else
        {
            return JsonSerializer.Serialize(new
            {
                error = "Provide code (UN/LOCODE) or terminal_id"
            });
        }

        if (terminal is null)
        {
            return JsonSerializer.Serialize(new
            {
                error = $"No terminal found for {(string.IsNullOrEmpty(code) ? idRaw : code)}"
            });
        }

        return JsonSerializer.Serialize(new
        {
            id = terminal.Id,
            name = terminal.Name,
            code = terminal.Code,
            type = terminal.Type.ToString(),
            type_description = terminal.Type.GetDescription(),
            country_code = terminal.CountryCode,
            address = FormatAddress(terminal.Address),
            city = terminal.Address.City,
            notes = terminal.Notes
        });
    }

    /// <summary>
    /// Single-line address. <see cref="Address"/> is a record, and its default <c>ToString</c> emits
    /// property syntax - too many tokens for what it says.
    /// </summary>
    internal static string FormatAddress(Address address)
    {
        string[] parts =
        [
            address.Line1,
            address.Line2 ?? "",
            address.City,
            address.State,
            address.ZipCode,
            address.Country
        ];

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
