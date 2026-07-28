using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CreateLoadInvoiceTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "create_load_invoice";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("load_id") is not { } loadId)
            return JsonSerializer.Serialize(new { error = "Invalid or missing load_id" });

        var loadResult = await mediator.Send(new GetLoadByIdQuery { Id = loadId }, ct);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return JsonSerializer.Serialize(new { error = loadResult.Error ?? "Load not found" });

        var load = loadResult.Value;
        if (load.Customer is null)
            return JsonSerializer.Serialize(new { error = "The load has no customer to invoice" });

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
            return JsonSerializer.Serialize(new { error = "Invoice amount must be greater than zero" });

        var result = await mediator.Send(new CreateLoadInvoiceCommand
        {
            CustomerId = load.Customer.Id,
            LoadId = loadId,
            PaymentAmount = amount,
            // The customer has not paid yet - settled later via a payment link.
            RecordPayment = false
        }, ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { success = false, error = result.Error });

        // The command returns no id; look it up so the conversation can reference the invoice.
        var created = await mediator.Send(new GetInvoicesQuery
        {
            LoadId = loadId,
            InvoiceType = InvoiceType.Load,
            OrderBy = "-CreatedDate",
            Page = 1,
            PageSize = 1
        }, ct);

        var invoice = created.Value?.FirstOrDefault();
        return JsonSerializer.Serialize(new
        {
            success = true,
            invoice_id = invoice?.Id,
            invoice_number = invoice?.Number,
            amount,
            currency = invoice?.Total.Currency,
            load_number = load.Number,
            customer = load.Customer.Name
        });
    }
}
