using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Loads.Queries;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetLoadTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_load";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("load_id") is not { } loadId)
            return ToolResult.Error("Invalid or missing load_id");

        var result = await mediator.Send(new GetLoadByIdQuery { Id = loadId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load not found");

        var l = result.Value;
        return JsonSerializer.Serialize(new
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
