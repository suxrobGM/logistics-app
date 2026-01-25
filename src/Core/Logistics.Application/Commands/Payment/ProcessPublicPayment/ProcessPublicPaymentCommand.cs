using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Processes a payment via a public payment link.
/// Creates a Stripe PaymentIntent with destination charges.
/// </summary>
public record ProcessPublicPaymentCommand : IAppRequest<Result<ProcessPublicPaymentResult>>
{
    public required Guid TenantId { get; set; }
    public required string Token { get; set; }

    /// <summary>
    /// Amount to pay. If null, pays the full amount due.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Stripe PaymentMethod ID from the frontend.
    /// </summary>
    public required string StripePaymentMethodId { get; set; }
}

public record ProcessPublicPaymentResult
{
    public required string PaymentIntentId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Status { get; set; }
}
