using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class GetInvoiceTool(IMediator mediator)
    : AgentTool<GetInvoiceTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The invoice ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Invoice)]
        public required Guid InvoiceId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_invoice",
        "Get one invoice by ID: status, totals, amount paid, due date, send history, customer (with email), and the load it bills.")
    {
        RequiredFeature = TenantFeature.Invoices,
        RequiredPermission = Permission.Invoice.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };


    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery { Id = input.InvoiceId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Invoice not found");

        var i = result.Value;
        var payments = i.Payments.ToList();
        return ToolResult.Ok(new
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
