using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

/// <summary>
/// Computes tax for a hypothetical set of line items without persisting anything.
/// Backs the create/edit-invoice form's live recalc + the AI dispatch
/// <c>preview_tax_calculation</c> tool.
/// </summary>
public record PreviewInvoiceTaxQuery : IQuery<Result<PreviewInvoiceTaxResponse>>
{
    public required PreviewInvoiceTaxRequest Request { get; init; }
}
