using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Creates a public payment link for an invoice.
/// </summary>
public record CreatePaymentLinkCommand : IAppRequest<Result<PaymentLinkDto>>
{
    public required Guid InvoiceId { get; set; }

    /// <summary>
    /// Number of days until the link expires. Default is 30 days.
    /// </summary>
    public int ExpirationDays { get; set; } = 30;
}
