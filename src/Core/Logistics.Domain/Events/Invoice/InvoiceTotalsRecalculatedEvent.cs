using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised whenever an invoice's <c>Subtotal</c>, <c>TaxTotal</c>, or <c>Total</c> changes
/// as a result of <see cref="Entities.Invoice.RecalculateTotals"/>.
/// </summary>
public record InvoiceTotalsRecalculatedEvent(
    Guid InvoiceId,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    string Currency,
    TaxBehavior TaxBehavior) : IDomainEvent;
