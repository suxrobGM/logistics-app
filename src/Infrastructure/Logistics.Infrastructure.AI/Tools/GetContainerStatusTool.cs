using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Looks up an intermodal container by ISO 6346 number (or id), so the agent knows where the box is
/// before assigning the load that carries it.
/// </summary>
internal sealed class GetContainerStatusTool(ITenantUnitOfWork tenantUow) : IAiDispatchTool
{
    public string Name => "get_container_status";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var number = input["container_number"]?.GetValue<string>()?.Trim();
        var idRaw = input["container_id"]?.GetValue<string>();

        Container? container;

        if (!string.IsNullOrEmpty(number))
        {
            // Dispatchers type ISO 6346 numbers in either case, but stored numbers are canonical -
            // so normalise the term and compare exactly, which the unique index can serve.
            var normalized = Container.NormalizeNumber(number);
            container = await tenantUow.Repository<Container>()
                .GetAsync(c => c.Number == normalized, ct);
        }
        else if (Guid.TryParse(idRaw, out var containerId))
        {
            container = await tenantUow.Repository<Container>().GetByIdAsync(containerId, ct);
        }
        else
        {
            return JsonSerializer.Serialize(new
            {
                error = "Provide container_number (ISO 6346) or container_id"
            });
        }

        if (container is null)
        {
            return JsonSerializer.Serialize(new
            {
                error = $"No container found for {(string.IsNullOrEmpty(number) ? idRaw : number)}"
            });
        }

        // Container has no Loads collection - the link is one-way from Load.ContainerId.
        var load = await tenantUow.Repository<Load>()
            .GetAsync(l => l.ContainerId == container.Id, ct);

        var terminal = container.CurrentTerminal;

        return JsonSerializer.Serialize(new
        {
            id = container.Id,
            number = container.Number,
            iso_type = container.IsoType.ToString(),
            iso_type_description = container.IsoType.GetDescription(),
            status = container.Status.ToString(),
            is_laden = container.IsLaden,
            gross_weight = container.GrossWeight,
            seal_number = container.SealNumber,
            booking_reference = container.BookingReference,
            bill_of_lading_number = container.BillOfLadingNumber,
            notes = container.Notes,
            current_terminal = terminal is null
                ? null
                : new
                {
                    id = terminal.Id,
                    name = terminal.Name,
                    code = terminal.Code,
                    type = terminal.Type.ToString(),
                    country_code = terminal.CountryCode,
                    address = GetTerminalInfoTool.FormatAddress(terminal.Address)
                },
            loaded_at = container.LoadedAt,
            delivered_at = container.DeliveredAt,
            returned_at = container.ReturnedAt,
            linked_load = load is null
                ? null
                : new
                {
                    id = load.Id,
                    number = load.Number,
                    name = load.Name,
                    status = load.Status.ToString()
                }
        });
    }
}
