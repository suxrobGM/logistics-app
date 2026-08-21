using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class CreatePaymentLinkTool(IMediator mediator)
    : AgentTool<CreatePaymentLinkTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The invoice ID (GUID) to create the link for")]
        [AgentEntityId(AgentEntityKind.Invoice)]
        public required Guid InvoiceId { get; init; }

        [Description("Brief explanation of why this link should be created")]
        public required string Reasoning { get; init; }

        [Description("Days until the link expires (default 30)")]
        public int? ExpirationDays { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "create_payment_link",
        "Create a public payment link URL for an invoice without emailing anything. Use when the user wants a link to share themselves; send_invoice already includes one.")
    {
        RequiredFeature = TenantFeature.Payments,
        RequiredPermission = Permission.Payment.Manage,
        DecisionType = AgentDecisionType.CreatePaymentLink
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePaymentLinkCommand
        {
            InvoiceId = input.InvoiceId,
            ExpirationDays = input.ExpirationDays ?? 30
        }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.WriteFailed(result);

        var link = result.Value;
        return ToolResult.Ok(new
        {
            success = true,
            invoice_id = input.InvoiceId,
            url = link.Url,
            expires_at = link.ExpiresAt.ToString("yyyy-MM-dd")
        });
    }
}
