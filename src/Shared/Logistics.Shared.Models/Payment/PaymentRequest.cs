
namespace Logistics.Shared.Models;

/// <summary>
/// Request body for processing a public payment.
/// </summary>
public record ProcessPublicPaymentRequest
{
    /// <summary>
    /// Stripe PaymentMethod ID from the frontend (created via Stripe Elements).
    /// </summary>
    public required string PaymentMethodId { get; set; }

    /// <summary>
    /// Amount to pay. If null, pays the full amount due.
    /// </summary>
    public decimal? Amount { get; set; }
}
