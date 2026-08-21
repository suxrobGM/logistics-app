using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class GetLoadTool(IMediator mediator)
    : AgentTool<GetLoadTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The load ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_load",
        "Get one load by ID: status, addresses, delivery cost, customer (with email), assigned truck, delivery timestamps, and whether it already has an invoice. ALWAYS call this before create_load_invoice - it supplies the delivery cost and shows whether the load is Delivered.")
    {
        RequiredFeature = TenantFeature.Loads,
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };


    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoadByIdQuery { Id = input.LoadId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load not found");

        var l = result.Value;
        return ToolResult.Ok(new
        {
            id = l.Id,
            number = l.Number,
            name = l.Name,
            status = l.Status.ToString(),
            type = l.Type.ToString(),
            origin = l.OriginAddress.ToString(),
            destination = l.DestinationAddress.ToString(),
            distance_km = Math.Round(DispatchUnits.MetersToKm(l.Distance), 1),
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name,
            customer_id = l.Customer?.Id,
            customer_email = l.Customer?.Email,
            truck_number = l.AssignedTruckNumber,
            dispatched_at = l.DispatchedAt?.ToString("yyyy-MM-dd HH:mm"),
            delivered_at = l.DeliveredAt?.ToString("yyyy-MM-dd HH:mm"),
            has_invoice = l.Invoice is not null,
            invoice_id = l.Invoice?.Id,
            invoice_status = l.Invoice?.Status.ToString()
        });
    }
}
