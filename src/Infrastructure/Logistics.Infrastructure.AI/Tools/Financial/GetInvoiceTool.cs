using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Queries;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetInvoiceTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_invoice";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("invoice_id") is not { } invoiceId)
            return ToolResult.Error("Invalid or missing invoice_id");

        var result = await mediator.Send(new GetInvoiceByIdQuery { Id = invoiceId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Invoice not found");

        var i = result.Value;
        var payments = i.Payments.ToList();
        return JsonSerializer.Serialize(new
        {
            id = i.Id,
            number = i.Number,
            type = i.Type.ToString(),
            status = i.Status.ToString(),
            subtotal = i.Subtotal.Amount,
            tax_total = i.TaxTotal.Amount,
            total = i.Total.Amount,
            currency = i.Total.Currency,
            amount_paid = payments.Sum(p => p.Amount.Amount),
            payment_count = payments.Count,
            due_date = i.DueDate?.ToString("yyyy-MM-dd"),
            sent_at = i.SentAt?.ToString("yyyy-MM-dd HH:mm"),
            sent_to = i.SentToEmail,
            load_number = i.LoadNumber,
            load_id = i.LoadId,
            customer = i.Customer?.Name,
            customer_id = i.CustomerId,
            customer_email = i.Customer?.Email,
            notes = i.Notes,
            created_at = i.CreatedDate.ToString("yyyy-MM-dd")
        });
    }
}
