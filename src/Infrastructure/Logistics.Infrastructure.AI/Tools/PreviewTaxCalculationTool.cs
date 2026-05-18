using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Logistics.Application.Modules.Financial.Invoices.Queries;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Computes per-line tax + breakdown for a hypothetical set of line items without persisting.
/// Read tool: safe to call in any mode. Backed by <c>PreviewInvoiceTaxQuery</c>, which routes
/// through the configured <c>ITaxCalculator</c> (Stripe Tax in production, Manual fallback).
/// </summary>
internal sealed class PreviewTaxCalculationTool(IMediator mediator) : IAiDispatchTool
{
    public string Name => "preview_tax_calculation";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["customer_id"]?.GetValue<string>(), out var customerId))
        {
            return Error("Missing or invalid customer_id");
        }

        var currency = input["currency"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(currency))
        {
            return Error("Missing currency");
        }

        if (input["line_items"] is not JsonArray itemsArray || itemsArray.Count == 0)
        {
            return Error("Missing or empty line_items");
        }

        var lineItems = new List<PreviewInvoiceTaxLineItem>(itemsArray.Count);
        foreach (var item in itemsArray)
        {
            if (item is null) continue;
            if (!decimal.TryParse(item["amount"]?.ToString(), out var amount))
            {
                return Error("Each line_item requires a numeric amount");
            }

            lineItems.Add(new PreviewInvoiceTaxLineItem
            {
                Description = item["description"]?.GetValue<string>() ?? "Item",
                Type = ParseLineItemType(item["type"]?.GetValue<string>()),
                Amount = amount,
                Quantity = item["quantity"]?.GetValue<int>() ?? 1,
                TaxCode = item["tax_code"]?.GetValue<string>()
            });
        }

        var result = await mediator.Send(new PreviewInvoiceTaxQuery
        {
            Request = new PreviewInvoiceTaxRequest
            {
                CustomerId = customerId,
                Currency = currency,
                LineItems = lineItems
            }
        }, ct);

        if (!result.IsSuccess || result.Value is null)
        {
            return Error(result.Error ?? "Unknown error");
        }

        var response = result.Value;
        return JsonSerializer.Serialize(new
        {
            tax_behavior = response.TaxBehavior.ToString(),
            currency,
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

    private static InvoiceLineItemType ParseLineItemType(string? raw) =>
        Enum.TryParse<InvoiceLineItemType>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : InvoiceLineItemType.Other;

    private static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message });
}
