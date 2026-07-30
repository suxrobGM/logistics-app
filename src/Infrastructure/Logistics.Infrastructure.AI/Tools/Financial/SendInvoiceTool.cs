using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Commands;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class SendInvoiceTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "send_invoice";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("invoice_id") is not { } invoiceId)
            return ToolResult.Error("Invalid or missing invoice_id");

        var recipientEmail = input.GetString("recipient_email");
        if (string.IsNullOrWhiteSpace(recipientEmail))
            return ToolResult.Error("Invalid or missing recipient_email");

        var result = await mediator.Send(new SendInvoiceCommand
        {
            InvoiceId = invoiceId,
            RecipientEmail = recipientEmail,
            PersonalMessage = input.GetString("personal_message")
        }, ct);

        return ToolResult.Written(result, new { success = true, invoice_id = invoiceId, sent_to = recipientEmail });
    }
}
