using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Events;

/// <summary>Raised by <see cref="Entities.Invoice.RecalculateTotals"/> when the totals change.</summary>
public record InvoiceTotalsRecalculatedEvent(
    Guid InvoiceId,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    TaxBehavior TaxBehavior) : IDomainEvent;
