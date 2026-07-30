using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.Operations.Loads.Queries;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class CreateLoadInvoiceTool(IMediator mediator) : IAgentTool
{
    public string Name => "create_load_invoice";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("load_id") is not { } loadId)
            return ToolResult.Error("Invalid or missing load_id");

        var loadResult = await mediator.Send(new GetLoadByIdQuery { Id = loadId }, ct);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return ToolResult.Error(loadResult.Error ?? "Load not found");

        var load = loadResult.Value;
        if (load.Customer is null)
            return ToolResult.Error("The load has no customer to invoice");

        if (load.Invoice is not null)
        {
            return JsonSerializer.Serialize(new
            {
                error = "The load already has an invoice",
                invoice_id = load.Invoice.Id,
                invoice_status = load.Invoice.Status.ToString()
            });
        }

        var amount = input.GetDecimal("amount") ?? load.DeliveryCost;
        if (amount <= 0)
            return ToolResult.Error("Invoice amount must be greater than zero");

        var result = await mediator.Send(new CreateLoadInvoiceCommand
        {
            CustomerId = load.Customer.Id,
            LoadId = loadId,
            PaymentAmount = amount,
            // The customer has not paid yet - settled later via a payment link.
            RecordPayment = false
        }, ct);

        if (!result.IsSuccess)
            return ToolResult.WriteFailed(result);

        // Read the created invoice back for its number and currency, which the command does not return.
        var created = await mediator.Send(new GetInvoiceByIdQuery { Id = result.Value }, ct);

        var invoice = created.Value;
        return JsonSerializer.Serialize(new
        {
            success = true,
            invoice_id = result.Value,
            invoice_number = invoice?.Number,
            amount,
            currency = invoice?.Total.Currency,
            load_number = load.Number,
            customer = load.Customer.Name
        });
    }
}
