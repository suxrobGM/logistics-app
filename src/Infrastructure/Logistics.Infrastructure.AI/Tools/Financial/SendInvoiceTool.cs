using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class SendInvoiceTool(IMediator mediator)
    : AgentTool<SendInvoiceTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The invoice ID (GUID) to send")]
        [AgentEntityId(AgentEntityKind.Invoice)]
        public required Guid InvoiceId { get; init; }

        [Description("Recipient email address - use the customer's email from get_load or get_invoice unless the user gives another")]
        public required string RecipientEmail { get; init; }

        [Description("Brief explanation of why this invoice should be sent")]
        public required string Reasoning { get; init; }

        [Description("Optional note included in the email body")]
        public string? PersonalMessage { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "send_invoice",
        "Email an invoice to a recipient. This mints a 30-day payment link and includes it in the email - do NOT also call create_payment_link for the same invoice.")
    {
        RequiredFeature = TenantFeature.Invoices,
        RequiredPermission = Permission.Invoice.Manage,
        DecisionType = AgentDecisionType.SendInvoice
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new SendInvoiceCommand
        {
            InvoiceId = input.InvoiceId,
            RecipientEmail = input.RecipientEmail,
            PersonalMessage = input.PersonalMessage
        }, ct);

        return ToolResult.Written(result,
            new { success = true, invoice_id = input.InvoiceId, sent_to = input.RecipientEmail });
    }
}
