using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Container = Logistics.Domain.Entities.Container;

namespace Logistics.Infrastructure.AI.Tools.Intermodal;

/// <summary>
/// Looks up an intermodal container by ISO 6346 number (or id), so the agent knows where the box is
/// before assigning the load that carries it.
/// </summary>
internal sealed class GetContainerStatusTool(ITenantUnitOfWork tenantUow)
    : AgentTool<GetContainerStatusTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("ISO 6346 container number, e.g. 'MSCU1234567'")]
        public string? ContainerNumber { get; init; }

        [Description("Container ID (GUID) - use only when the number is unknown")]
        public Guid? ContainerId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_container_status",
        "Look up an intermodal container by its ISO 6346 number. Returns status (Empty, Loaded, AtPort, InTransit, Delivered, Returned), ISO type, laden flag, gross weight, seal, booking reference, bill of lading, the terminal the box is currently at, and the load it is linked to. Call this for any load that reports a container_number before assigning it.")
    {
        RequiredFeature = TenantFeature.IntermodalContainers,
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var number = input.ContainerNumber?.Trim();
        Container? container;

        if (!string.IsNullOrEmpty(number))
        {
            // Dispatchers type ISO 6346 numbers in either case, but stored numbers are canonical -
            // so normalise the term and compare exactly, which the unique index can serve.
            var normalized = Container.NormalizeNumber(number);
            container = await tenantUow.Repository<Container>()
                .GetAsync(c => c.Number == normalized, ct);
        }
        else if (input.ContainerId is { } containerId)
        {
            container = await tenantUow.Repository<Container>().GetByIdAsync(containerId, ct);
        }
        else
        {
            return ToolResult.Error("Provide container_number (ISO 6346) or container_id");
        }

        if (container is null)
            return ToolResult.Error($"No container found for {number ?? input.ContainerId?.ToString()}");

        // Container has no Loads collection - the link is one-way from Load.ContainerId.
        var load = await tenantUow.Repository<Load>()
            .GetAsync(l => l.ContainerId == container.Id, ct);

        var terminal = container.CurrentTerminal;

        return ToolResult.Ok(new
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
