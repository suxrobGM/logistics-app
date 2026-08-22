using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Financial;

/// <summary>
/// Computes per-line tax + breakdown for a hypothetical set of line items without persisting.
/// Read tool: safe to call in any mode. Backed by <c>PreviewInvoiceTaxQuery</c>, which routes
/// through the configured <c>ITaxCalculator</c> (Stripe Tax in production, Manual fallback).
/// </summary>
internal sealed class PreviewTaxCalculationTool(IMediator mediator)
    : AgentTool<PreviewTaxCalculationTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The customer ID (GUID) - drives jurisdiction + reverse-charge")]
        [AgentEntityId(AgentEntityKind.Customer)]
        public required Guid CustomerId { get; init; }

        [Description("ISO-4217 currency code, e.g. 'USD' or 'EUR'")]
        public required string Currency { get; init; }

        [Description("Line items to score")]
        public required LineItem[] LineItems { get; init; }
    }

    internal sealed record LineItem
    {
        [Description("Human-readable label for the line")]
        public required string Description { get; init; }

        [Description("Per-unit net amount in the invoice currency")]
        public required decimal Amount { get; init; }

        [Description("The kind of charge this line represents")]
        public InvoiceLineItemType? Type { get; init; }

        [Description("Quantity (defaults to 1)")]
        public int? Quantity { get; init; }

        [Description("Optional Stripe Tax product code (txcd_*); leave blank to use the tenant default")]
        public string? TaxCode { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "preview_tax_calculation",
        "Compute VAT / sales tax / GST for a hypothetical set of line items without persisting an invoice. Returns per-line tax amount, aggregate breakdown by jurisdiction, and reverse-charge / not-collecting flags. Use when quoting a customer or sanity-checking that tax setup will work before creating the invoice. Read-only.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (input.LineItems.Length == 0)
            return ToolResult.Error("Missing or empty line_items");

        var lineItems = input.LineItems.Select(item => new PreviewInvoiceTaxLineItem
        {
            Description = item.Description,
            Type = item.Type ?? InvoiceLineItemType.Other,
            Amount = item.Amount,
            Quantity = item.Quantity ?? 1,
            TaxCode = item.TaxCode
        }).ToList();

        var result = await mediator.Send(new PreviewInvoiceTaxQuery
        {
            Request = new PreviewInvoiceTaxRequest
            {
                CustomerId = input.CustomerId,
                Currency = input.Currency,
                LineItems = lineItems
            }
        }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Unknown error");

        var response = result.Value;
        return ToolResult.Ok(new
        {
            tax_behavior = response.TaxBehavior.ToString(),
            currency = input.Currency,
            subtotal = response.Subtotal.Amount,
            tax_total = response.TaxTotal.Amount,
            total = response.Total.Amount,
            warning = response.Warning,
            lines = response.Lines.Select(l => new
            {
                line_item_id = l.LineItemId,
                rate_percent = l.RatePercent,
                tax_amount = l.TaxAmount,
                tax_code = l.TaxCode
            }),
            breakdown = response.Breakdown.Select(b => new
            {
                rate_percent = b.RatePercent,
                base_amount = b.BaseAmount,
                tax_amount = b.TaxAmount,
                jurisdiction = b.Jurisdiction.Region is null
                    ? b.Jurisdiction.CountryCode
                    : $"{b.Jurisdiction.CountryCode}-{b.Jurisdiction.Region}",
                description = b.Description
            })
        });
    }
}
