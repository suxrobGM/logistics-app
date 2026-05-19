using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Financial.Tax.Services;

/// <summary>
/// Bridges <see cref="ITaxCalculator"/> with an <see cref="Invoice"/>: builds the request
/// from tenant + invoice context, applies per-line tax + breakdown, then calls
/// <see cref="Invoice.RecalculateTotals"/>. Must be called after every line-item mutation.
/// </summary>
public interface IInvoiceTaxApplier : IApplicationService
{
    /// <summary>
    /// Applies tax to <paramref name="invoice"/> in place. Only <see cref="LoadInvoice"/>
    /// hits the calculator (subscription/payroll invoices have no external customer);
    /// for other types it just calls <see cref="Invoice.RecalculateTotals"/>.
    /// </summary>
    Task ApplyAsync(Invoice invoice, CancellationToken ct = default);
}
