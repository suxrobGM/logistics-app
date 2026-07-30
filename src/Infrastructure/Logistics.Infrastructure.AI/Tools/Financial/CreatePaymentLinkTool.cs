using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class CreatePaymentLinkTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "create_payment_link";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("invoice_id") is not { } invoiceId)
            return ToolResult.Error("Invalid or missing invoice_id");

        var result = await mediator.Send(new CreatePaymentLinkCommand
        {
            InvoiceId = invoiceId,
            ExpirationDays = input.GetInt("expiration_days") ?? 30
        }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.WriteFailed(result);

        var link = result.Value;
        return JsonSerializer.Serialize(new
        {
            success = true,
            invoice_id = invoiceId,
            url = link.Url,
            expires_at = link.ExpiresAt.ToString("yyyy-MM-dd")
        });
    }
}
