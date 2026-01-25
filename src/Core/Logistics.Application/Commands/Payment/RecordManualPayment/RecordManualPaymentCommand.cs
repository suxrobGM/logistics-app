using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Records a manual payment (cash or check) for an invoice.
/// Only Owner and Manager roles can record manual payments.
/// </summary>
public record RecordManualPaymentCommand : IAppRequest<Result>
{
    public required Guid InvoiceId { get; set; }
    public required decimal Amount { get; set; }
    public required PaymentMethodType Type { get; set; }

    /// <summary>
    /// Reference number (e.g., check number).
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes about the payment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date when the payment was received. Defaults to now if not specified.
    /// </summary>
    public DateTime? ReceivedDate { get; set; }
}
